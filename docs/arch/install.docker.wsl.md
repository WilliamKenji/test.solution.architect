# Instalação do docker no WSL

## 1. Atualize os pacotes do sistema:
```
sudo apt update && sudo apt upgrade -y
```

## 2. Instale pacotes necessários (HTTPS, curl, etc.):
```
sudo apt install -y apt-transport-https ca-certificates curl software-properties-common gnupg lsb-release
```

## 3. Adicione a chave GPG oficial do Docker:
```
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg
```

## 4. Adicione o repositório oficial do Docker (ajusta automaticamente para sua versão de Ubuntu):

```
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```

## 5. Atualize novamente e instale o Docker + Compose plugin:
```
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

## 6. Adicione seu usuário ao grupo docker (para rodar sem sudo):
```
sudo usermod -aG docker $USER
newgrp docker
```

## 7. Inicie o serviço Docker (no WSL2 sem systemd por padrão):
```
sudo service docker start
```

## 8. Verifique a instalação:
```
docker --version
docker compose version   # (note: é "docker compose", não "docker-compose")
docker run hello-world
```
