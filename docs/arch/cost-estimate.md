## Estimativa de Custos Mensais - Infraestrutura Azure (Brazil South) para Ambientes Dev, Stg e Prod

Estimativa com AKS (Kubernetes) + migração gradual de legado.  
Uso: baixo-médio (picos ocasionais de 50 RPS no Consolidated Services apenas em Prod, ~10k-50k eventos/dia via Service Bus em Prod; Dev/Stg com uso baixo/testes).  
Região: Brazil South (pay-as-you-go, sem reservas ou Savings Plan).  
Moeda: BRL (conversão aproximada USD → BRL ~5.7 em 2026).  
Valores aproximados (verifique sempre no [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)).

Premissas gerais:
- **Prod**: Tiers Premium/Enterprise para alta disponibilidade, zone-redundancy, escalabilidade (50 RPS).
- **Stg**: Tiers Standard/Premium mais baixos, redundância mínima, autoscaling limitado; uso para testes de integração.
- **Dev**: Tiers Basic/Standard, 1 instância/node, sem redundância; uso local/desenvolvedores (pode ser desligado off-hours para economia extra).
- Sem licenças extras (todas managed/open-source).
- CI/CD via Azure Pipelines + ACR compartilhado entre ambientes (custo baixo, distribuído).

| Recurso                                      | Configuração Dev (Tier Baixo)                                  | Custo Mensal Dev (BRL) | Configuração Stg (Tier Médio)                                  | Custo Mensal Stg (BRL) | Configuração Prod (Tier Alto)                                  | Custo Mensal Prod (BRL) | Observações / Trade-offs |
|----------------------------------------------|----------------------------------------------------------------|------------------------|----------------------------------------------------------------|------------------------|----------------------------------------------------------------|-------------------------|--------------------------|
| Azure Kubernetes Service (AKS)               | 1 node básico (ex.: D2s v5, sem autoscaling/zone-redundancy)   | R$ 500 – 1.000        | 1-2 nodes (D4s v5, autoscaling limitado, sem zone-redundancy)  | R$ 1.000 – 2.000      | 2-3 nodes (D4s v5, autoscaling completo, zone-redundant)      | R$ 3.000 – 6.000       | Dev/Stg: Menos compute, sem HA; Prod: Escala para picos. |
| Azure Container Registry (ACR)               | Basic tier (push/pull básico)                                  | R$ 100 – 300          | Standard tier (VNet básico)                                    | R$ 200 – 500          | Premium tier (geo-replication, VNet)                           | R$ 200 – 800           | Compartilhado; custo por GB + operações. |
| Azure Application Gateway + WAF v2           | Basic (1 CU, sem WAF/zone)                                     | R$ 200 – 500          | Standard (1-2 CU, WAF básico)                                  | R$ 400 – 800          | Premium (2-3 CU, WAF v2, zone-redundant)                       | R$ 800 – 1.800         | Dev: Sem WAF para economia; Stg/Prod: Segurança essencial. |
| Azure API Management (APIM)                  | Developer tier (sem VNet, limitações)                          | R$ 500 – 1.000        | Standard v2 (VNet básico, rate limiting)                       | R$ 2.000 – 4.000      | Premium (VNet-integrated, zone-redundant)                      | R$ 12.000 – 18.000     | Dev: Barato, mas sem features avançadas; Prod: Alta SLA. |
| Azure SQL Database (Elastic Pool)            | Single Database Basic (5 DTUs)                                 | R$ 300 – 600          | Elastic Pool Standard (50-100 DTUs)                            | R$ 800 – 1.500        | Elastic Pool Premium (100-200 vCores/eDTUs, zone-redundant)    | R$ 3.000 – 7.000       | Dev: Básico para testes; Prod: Pool para cargas variáveis. |
| Azure Service Bus (Premium)                  | Basic tier (sem sessões/duplicação)                            | R$ 200 – 500          | Standard tier (messaging units básicos)                        | R$ 500 – 1.000        | Premium (zone-redundant, sessões/dead-letter)                  | R$ 1.500 – 4.000       | Dev/Stg: Sem features avançadas; Prod: Resiliência para picos. |
| Azure Cache for Redis                        | Basic (250 MB, sem redundância)                                | R$ 200 – 500          | Standard (1-6 GB, zone básico)                                 | R$ 500 – 1.000        | Premium/Enterprise (13-26 GB, zone-redundant)                  | R$ 1.500 – 4.000       | Dev: Cache mínimo; Prod: Alta performance para saldos. |
| Azure Key Vault                              | Standard                                                       | R$ 50 – 150           | Standard                                                       | R$ 50 – 150           | Premium (HSM-backed)                                           | R$ 100 – 400           | Baixo custo em todos; essencial para secrets. |
| Application Insights / Azure Monitor         | Uso básico                                                     | R$ 100 – 300          | Uso básico                                                     | R$ 100 – 300          | Uso completo (alertas, traces)                                 | R$ 300 – 800           | Custo por GB; compartilhado. |
| Outros (Private Endpoints, VNet, DNS, Defender for Cloud, Activity Log, etc.) | Mínimo (sem Defender)                                         | R$ 100 – 300          | Básico (com Activity Log)                                      | R$ 200 – 500          | Completo (Defender, Private DNS)                               | R$ 500 – 1.500         | Dev: Sem extras; Prod: Segurança full. |
| **Total Estimado Inicial por Ambiente**      | -                                                              | **R$ 2.250 – 5.150**  | -                                                              | **R$ 5.750 – 12.150** | -                                                              | **R$ 23.000 – 45.000** | Totais aproximados; economia em Dev/Stg em tiers com custo menor. |

**Premissas adicionais por ambiente**:
- **Dev**: Foco em desenvolvimento local (pode rodar em máquinas dev, mas estimado Azure full); desligar recursos noturnos/fins de semana → redução extra 50%.
- **Stg**: Testes de staging (carga simulada 10-20% de Prod); sem zone-redundancy para economia.
- **Prod**: Alta disponibilidade, escalabilidade para 50 RPS.
- CI/CD: Compartilhado (Azure Pipelines + ACR) → custo distribuído (~R$500-1.000 total/mês).