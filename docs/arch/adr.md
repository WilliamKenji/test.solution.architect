# ADR-001 - Adotar Arquitetura de Microservices Event-Driven com Azure para Migração Gradual de Monolito Legado

## Status
Accepted

## Data
2026-02-17

## Contexto
Realizar o controle de fluxo de caixa diário, com dois serviços principais:  
- Controle de lançamentos (débitos/créditos)  
- Consolidado diário de saldo  

Requisitos obrigatórios incluem: segregação de capacidades, escalabilidade, alta disponibilidade, resiliência, segurança e desacoplamento.  
Requisitos não-funcionais:  
- Lançamentos independentes do consolidado (99.9% uptime isolado)  
- Consolidado suporta picos de 50 RPS com ≤5% perda  

O sistema parte de um **monolito legado** (hipotético, com lógica acoplada e DB compartilhado), e a migração deve ser gradual para minimizar riscos e downtime (Strangler Pattern).  
A solução deve ser flexível, reutilizável, alinhada à estratégia de negócios (crescimento do comerciante) e documentada com decisões justificadas.

## Decisão
Seguindo as diretrizes da Arquitetura Corporativa da Tech Stack, centrada no ecossistema Microsoft, foi adotada uma **arquitetura de microserviços event-driven**, hospedada de forma nativa e estratégica no **Azure**, estruturada nos seguintes elementos:

- Dois microservices principais em **.NET 8/9** (ASP.NET Core): Financial Transactions Service (lançamentos) e Consolidated Service (saldos diários).  
- **Azure Service Bus** (Premium) como broker para eventos assíncronos (decoupling via publish/subscribe).  
- **Azure SQL Database** (Elastic Pool) para persistência relacional.  
- **Azure Cache for Redis** para cache de saldos diários (performance em picos).  
- Exposição segura via **Azure Application Gateway + WAF** → **Azure API Management (APIM)** com Private Endpoints, Policies e JWT.  
- Migração incremental usando **Strangler Pattern** + **Anti-Corruption Layer (ACL)** durante transição.  

Deploy em AKS com CI/CD via Azure Devops e observabilidade via Application Insights.

## Considerações de segurança
- **Autenticação e Autorização**: JWT via SSO (Microsoft Entra ID), APIM com policies nas APIs; roles (comerciante full access, visualizador read-only).
- **Proteção de Rede**: Private Endpoints + VNet para todos os backends (SQL, Service Bus, Redis); sem exposição pública exceto APIM/Gateway.
- **Criptografia**: HTTPS everywhere; Always Encrypted + TDE no Azure SQL; secrets em Azure Key Vault.
- **Proteção contra Ataques**: WAF v2 no Application Gateway (OWASP ruleset); rate limiting no APIM para mitigar DDoS/abuso.
- **Monitoramento de Segurança**: Application Insights + Azure Defender para alertas de anomalias; logs auditáveis.
- **Trade-off**: Overhead mínimo de latência (~20-50ms) por camadas de segurança, mas essencial para dados financeiros sensíveis.

## Alternativas Consideradas

1. **Manter ou refatorar monolito legado**  
   - Vantagens: Simplicidade inicial, menor curva de aprendizado.  
   - Desvantagens: Acoplamento alto viola independência; difícil escalar só o consolidado; não atende picos de 50 RPS sem replicar toda a app; difícil reutilização futura.  
   - Rejeitado: Não atende requisitos de segregação, resiliência e escalabilidade.

2. **Serverless puro (Azure Functions + Event Grid)**  
   - Vantagens: Auto-scaling nativo, simplicidade para eventos.  
   - Desvantagens: Limitações em stateful processing (consumidores long-running); cold starts podem afetar latência em picos; menos controle em .NET comparado a self-hosted; migração de legado mais complexa.  
   - Rejeitado: Trade-off desfavorável para workload com queries agregadas e necessidade de background workers persistentes.

3. **SOA com orquestração central (ex.: Azure Logic Apps)**  
   - Vantagens: Fluxos orquestrados visíveis.  
   - Desvantagens: Acoplamento via orquestrador cria single point of failure; menos resiliente que event-driven puro; overhead em latência.  
   - Rejeitado: Viola resiliência e independência exigidas.

4. **RabbitMQ/Kafka local ou self-hosted**  
   - Vantagens: Familiaridade, controle total.  
   - Desvantagens: Gerenciamento operacional alto (HA, scaling); custos de infra maiores; não aproveita Azure-native (Service Bus tem integração melhor com Entra ID, Private Link, etc.).  
   - Rejeitado: Preferimos managed services Azure para reduzir toil e alinhar com cloud-native.

## Consequências

**Positivas**  
- Decoupling total: Lançamentos continuam funcionando mesmo se Consolidated cair (Service Bus bufferiza eventos).  
- Escalabilidade: Auto-scaling horizontal + Redis cache atende 50 RPS com baixa perda.  
- Resiliência: Zone-redundant, circuit breakers (Polly no .NET), dead-letter queues.  
- Segurança: OWASP via WAF/APIM, Private Endpoints, Always Encrypted.  
- Reutilização: Serviços independentes facilitam integração futura (ex.: BI, ERP).  
- Migração controlada: Strangler + ACL minimiza riscos durante transição.

**Negativas / Trade-offs**  
- Complexidade operacional maior que monolito (mais serviços, mensageria).  
- Eventual consistency no consolidado (delay ≤10s aceitável para relatório diário).  
- Custo inicial mais alto (~R$31.000–62.300/mês pay-as-you-go Brasil South; otimiza com reservations).  
- Overhead de aprendizado em Azure-native services.

**Riscos Mitigados**  
- Downtime na migração: Strangler incremental + A/B testing via APIM.  
- Segurança: JWT + Private Endpoint + WAF.  
- Performance: Rate limiting no APIM + Redis.

## Decisores
- Arquiteto de Soluções: William Kenji

## Referências
- [Domain Mapping e Capacidades de Negócio](domain.md)  
- [Requisitos Refinados](requirements.md)  
- [CAF (Azure Cloud Adoption Framework)](https://learn.microsoft.com/pt-br/azure/cloud-adoption-framework/) 
