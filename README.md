# Programação de API com Microservices - RabbitMQ

## Sobre o Projeto

Este projeto implementa um sistema de troca de mensagens utilizando RabbitMQ como message broker, desenvolvido como parte do Checkpoint 5 da disciplina de Análise e Desenvolvimento de Sistemas (TDS) da FIAP. O sistema consiste em microserviços para envio, validação e recebimento de informações sobre frutas de época e dados de usuários.

## Arquitetura do Sistema

O sistema é composto por cinco microserviços:

1. **Sender1_Frutas**: Envia informações sobre frutas de época para validação
2. **Sender2_Usuario**: Envia dados de usuários para validação
3. **Validation**: Serviço central que valida as informações recebidas dos senders
4. **Receiver1_Frutas**: Recebe e processa as informações validadas sobre frutas
5. **Receiver2_Usuario**: Recebe e processa os dados validados de usuários

### Diagrama de Fluxo

```
+---------------+    FrutasToValidationKey    +-------------+    ValidatedFrutasKey    +-----------------+
| Sender1_Frutas| -------------------------> |  Validation  | -----------------------> | Receiver1_Frutas|
+---------------+                            |    Service   |                          +-----------------+
                                             |              |
+---------------+    UsuariosToValidationKey |              |    ValidatedUsuariosKey  +-----------------+
| Sender2_Usuario| ------------------------> |              | -----------------------> | Receiver2_Usuario|
+---------------+                            +-------------+                          +-----------------+
```

## Tecnologias Utilizadas

- .NET 8.0
- RabbitMQ 3 (via Docker)
- Docker Compose
- Visual Studio 2022

## Estrutura do Projeto

O projeto é organizado em várias camadas:

### Common

Contém classes e configurações compartilhadas entre todos os microserviços:

- **Models**: Define as entidades `Fruta` e `Usuario` e a classe genérica `Message<T>` para serialização/deserialização
- **RabbitMQ**: Configurações e classes de conexão assíncrona com o RabbitMQ

### Microserviços

Cada microserviço é um projeto console independente:

- **Sender1_Frutas**: Envia dados de frutas de época
- **Sender2_Usuario**: Envia dados de usuários
- **Validation**: 
  - Recebe mensagens dos senders
  - Valida os dados usando `FrutaValidator` e `UsuariosValidator`
  - Envia os resultados para os receivers
- **Receiver1_Frutas**: Recebe e exibe as informações validadas sobre frutas
- **Receiver2_Usuario**: Recebe e exibe os dados validados de usuários

## Configuração do RabbitMQ

O projeto utiliza as seguintes configurações do RabbitMQ:

### Exchanges
- `frutas_exchange`: Para mensagens relacionadas a frutas
- `usuarios_exchange`: Para mensagens relacionadas a usuários
- `validation_exchange`: Para mensagens validadas

### Filas (Queues)
- `frutas_to_validation_queue`: Fila para enviar frutas para validação
- `usuarios_to_validation_queue`: Fila para enviar usuários para validação
- `validated_frutas_queue`: Fila para frutas validadas
- `validated_usuarios_queue`: Fila para usuários validados

### Routing Keys
- `frutas.validation`: Para encaminhar frutas para validação
- `usuarios.validation`: Para encaminhar usuários para validação
- `frutas.validated`: Para encaminhar frutas validadas
- `usuarios.validated`: Para encaminhar usuários validados

## Instalação e Execução

### Pré-requisitos
- .NET 8.0 SDK
- Docker Desktop

### Configurando o RabbitMQ via Docker

1. No diretório raiz do projeto, execute o comando:
```bash
docker-compose up -d
```

Isso iniciará o RabbitMQ em um container nas portas:
- 5672 (AMQP)
- 15672 (Interface de gerenciamento HTTP)

### Interface de Gerenciamento do RabbitMQ

Acesse a interface de gerenciamento em: http://localhost:15672/
- **Usuário**: guest
- **Senha**: guest

### Compilando e Executando os Microserviços

1. Abra o arquivo de solução (.sln) no Visual Studio 2022
2. Compile a solução completa
3. Configure a inicialização de múltiplos projetos na solução para executar todos os projetos simultaneamente:
   - Validation
   - Receiver1_Frutas
   - Receiver2_Usuario
   - Sender1_Frutas
   - Sender2_Usuario

Para executar os projetos manualmente, abra múltiplas instâncias do Console e execute os comandos na seguinte ordem:

```bash
# 1. Inicie o service de validação primeiro
dotnet run --project ./Validation/Validation.csproj

# 2. Inicie os receivers
dotnet run --project ./Receiver1_Frutas/Receiver1_Frutas.csproj
dotnet run --project ./Receiver2_Usuario/Receiver2_Usuario.csproj

# 3. Inicie os senders
dotnet run --project ./Sender1_Frutas/Sender1_Frutas.csproj
dotnet run --project ./Sender2_Usuario/Sender2_Usuario.csproj
```

## Exemplo de Uso

### Enviando Informações de Frutas

No console do Sender1_Frutas:
1. Insira o nome da fruta (ex: "Manga")
2. Insira a descrição da fruta (ex: "Fruta tropical, doce e suculenta, típica do verão")
3. A data e hora são automaticamente registradas pelo sistema

### Enviando Dados de Usuário

No console do Sender2_Usuario:
1. Insira o nome completo do usuário (ex: "João Silva")
2. Insira o endereço (ex: "Rua das Flores, 123")
3. Insira o RG (ex: "12.345.678-9")
4. Insira o CPF (ex: "123.456.789-00")
5. A data e hora são automaticamente registradas pelo sistema

### Observando a Validação e Recebimento

- No console do Validation, você verá as mensagens sendo recebidas e validadas
- Nos consoles do Receiver1_Frutas e Receiver2_Usuario, você verá as mensagens validadas sendo recebidas e exibidas

## Detalhes da Implementação

### Validação de Frutas

A validação de frutas verifica:
- Se o nome não está vazio
- Se a descrição não está vazia
- Se a data/hora de solicitação é válida

### Validação de Usuários

A validação de usuários verifica:
- Se o nome completo não está vazio
- Se o endereço não está vazio
- Se o RG não está vazio
- Se o CPF não está vazio e tem formato válido (000.000.000-00 ou 00000000000)
- Se a data/hora de registro é válida

### Comunicação Assíncrona

O projeto utiliza a API assíncrona do RabbitMQ.Client para todas as operações, garantindo melhor performance e escalabilidade:
- `CreateAsync()` para criar conexões
- `CreateChannelAsync()` para criar canais
- `ExchangeDeclareAsync()`, `QueueDeclareAsync()`, `QueueBindAsync()` para configurar exchanges e filas
- `BasicPublishAsync()` para publicar mensagens
- `AsyncEventingBasicConsumer` para consumir mensagens assincronamente

## Observações

- Para encerrar os microserviços Sender1_Frutas e Sender2_Usuario, deixe o campo de nome/nome completo em branco e pressione Enter
- Para encerrar os microserviços Validation, Receiver1_Frutas e Receiver2_Usuario, pressione Enter no console respectivo

---

## Equipe
Nome: Beatriz Silva RM552600
Vitor Onofre Ramos RM553241
Pedro Henrique soares araujo - RM553801
