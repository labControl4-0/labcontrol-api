# [S4][BE-07] Implementar Registro de Dispositivos e Envio Push

## 📌 Visão Geral e Objetivo
Implementação do registro de dispositivos móveis associados aos usuários autenticados e o serviço de envio de notificações push via **Expo Push Notification Service**. Essa funcionalidade permite alertar operadores e gestores no celular caso ocorram anomalias em máquinas ou eventos críticos no parque fabril.

---

## 🛠️ O Que Foi Implementado

### 1. Modelo de Dados (`UserDevice.cs`)
- Entidade [`UserDevice`](../../Models/UserDevice.cs) armazenando:
  - `Id` (UUID)
  - `UserId` (FK para [`User`](../../Models/User.cs))
  - `DeviceToken` (Token Expo do dispositivo, ex: `ExponentPushToken[...]`)
  - `Platform` (android / ios / web)
  - `DeviceName` (Nome do aparelho, ex: "Samsung Galaxy A54")
  - `IsActive` (Booleano para controle de ativação)
  - `CreatedAt` e `UpdatedAt`
- Mapeamento e migration no EF Core (`AddUserDevices`) com índice composto único `(UserId, DeviceToken)` e exclusão em cascata.

### 2. DTOs
- [`RegisterDeviceDto`](../../DTOs/Device/RegisterDeviceDto.cs): Validação de entrada no registro de aparelho.
- [`DeviceResponseDto`](../../DTOs/Device/DeviceResponseDto.cs): Retorno de dados públicos do dispositivo cadastrado.
- [`SendPushNotificationDto`](../../DTOs/Push/SendPushNotificationDto.cs): Contrato contendo título, mensagem, `category`, `avisoId`, `origin` e dados adicionais (`additionalData`).

### 3. Camada de Repositório (`DeviceRepository.cs`)
- Métodos assíncronos:
  - `AddOrUpdateAsync`: Cadastra novo dispositivo ou reativa se já existir.
  - `GetDevicesByUserIdAsync`: Busca apenas os dispositivos ativos pertencentes àquele usuário.
  - `DeactivateOrRemoveAsync`: Desativa o dispositivo do usuário autenticado.
  - `DeactivateTokensAsync`: Desativação em lote de tokens expirados/inválidos reportados pelo Expo.

### 4. Camada de Serviço (`PushNotificationService.cs` & `DeviceService.cs`)
- **Integração Expo Push (`https://exp.host/--/api/v2/push/send`):**
  - Envio em lote com payload formatado.
  - **Retries limitados:** até 2 tentativas em caso de instabilidade na rede.
  - **Tratamento de Receipts/Tickets:** Lê a resposta do Expo e identifica erros como `DeviceNotRegistered` desativando automaticamente os tokens no banco.
  - **Segurança de Logs:** Implementado método `MaskToken()` que oculta os tokens nos logs (ex: `Exponent...456]`), garantindo que nenhum token real vaze no console.

### 5. Controller REST (`DevicesController.cs`)
- Todas as rotas são protegidas por JWT (`[Authorize]`):
  - `GET /api/devices`: Lista os dispositivos do usuário logado.
  - `POST /api/devices`: Registra ou atualiza o token do dispositivo do usuário logado.
  - `DELETE /api/devices?token=...`: Desvincula e desativa o dispositivo.
  - `POST /api/devices/test-push`: Dispara uma notificação de teste para o celular do usuário logado (usado para validação e testes).

---

## ✅ Critérios de Aceite Atendidos

| Critério do Card | Como Foi Cumprido |
| :--- | :--- |
| **Usuário só registra/remove seu dispositivo** | O `UserId` é extraído do token JWT (`ClaimsPrincipal`) e não de parâmetros na URL, impedindo que um usuário manipule tokens de outro. |
| **Destinatário depende de autorização** | Endpoints protegidos com `[Authorize]`. |
| **Payload contém categoria, avisoId e origem** | DTO `SendPushNotificationDto` possui os campos tipados enviados no nó `data` do Expo. |
| **Nenhum token aparece em logs públicos** | `PushNotificationService.MaskToken` mascara os identificadores nos logs de sistema. |
| **Tratar tokens inválidos e retries** | Tokens desinstalados (`DeviceNotRegistered`) são marcados como inativos automaticamente no banco. |

---

## 🧪 Como Testar e Explicar para o Grupo

### 1. Registrar um Dispositivo
- **Endpoint:** `POST /api/devices`
- **Header:** `Authorization: Bearer <SEU_TOKEN_JWT>`
- **Corpo da Requisição (JSON):**
```json
{
  "deviceToken": "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]",
  "platform": "android",
  "deviceName": "Celular do Operador 01"
}
```

### 2. Disparar Teste de Push
- **Endpoint:** `POST /api/devices/test-push`
- **Header:** `Authorization: Bearer <SEU_TOKEN_JWT>`
- **Corpo da Requisição (JSON):**
```json
{
  "title": "Alerta de Máquina",
  "message": "Temperatura crítica detectada na Máquina CNC 02",
  "category": "ALERTA",
  "avisoId": "123e4567-e89b-12d3-a456-426614174000",
  "origin": "LabControl-SensorMonitor"
}
```

