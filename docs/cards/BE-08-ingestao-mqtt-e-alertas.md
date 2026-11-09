# [S4][BE-08] Ingestão MQTT com Regras de Alertas e Disparo Push

## 📌 Visão Geral e Objetivo
Aprimoramento do serviço de ingestão contínua de telemetria via broker MQTT ([`MqttIntegrationService.cs`](../../Services/MqttIntegrationService.cs)). O serviço agora atua em tempo real validando os contratos das mensagens de telemetria industrial, deduplicando mensagens repetidas, garantindo reconexão automática e avaliando **limiares críticos** (temperatura, vibração, RPM, consumo de energia). Ao detectar uma anomalia, o sistema gera automaticamente um evento no banco e **dispara uma notificação push** no celular do responsável pela planta.

---

## 🛠️ O Que Foi Implementado

### 1. Resiliência e Reconexão Automática
- Configurado loop com política de backoff e reconexão contínua: caso o broker MQTT caia ou a rede oscile, o serviço reconecta automaticamente e re-inscreve no tópico (`lab/machine/metrics`) sem derrubar a API.

### 2. Validação Estrita do Contrato MQTT
- **Validação de Máquina:** O `machineId` deve ser um UUID válido e a máquina **precisa existir** no PostgreSQL. Mensagens com identificadores inexistentes são rejeitadas com log de aviso.
- **Timestamp Seguro:** Campos de data/hora (`timestamp`, `collectedAt`, `time`) são tratados exclusivamente como metadados temporais e **NUNCA são gravados como valor de métrica**. Suporta ISO 8601 e Unix Epoch (segundos e milissegundos).
- **Lista de Metadados Reservados:** Campos como `unit`, `units`, `deviceId`, `messageId` são filtrados para não poluir os dados numéricos.

### 3. Deduplicação de Mensagens
- Cache em memória com política de expiração (10 minutos) que identifica mensagens duplicadas geradas por QoS do MQTT ou falhas de rede através do `messageId` ou da combinação `machineId + timestamp + hash(payload)`.

### 4. Limiares de Alerta Configuráveis (`appsettings.json`)
Foram definidos limiares industriais padrão parametrizáveis:
```json
"AlertThresholds": {
  "MaxTemperature": 80.0,
  "MaxVibration": 5.0,
  "MaxRpm": 3500.0,
  "MaxEnergyUsage": 100.0
}
```

### 5. Integração com Fluxo de Alertas e Push Notification
- Ao violar um limiar:
  1. Cria um registro na tabela `MachineEvents` (ex: `ALERTA_TEMPERATURA` ou `ALERTA_VIBRACAO`).
  2. Localiza o usuário dono da planta onde a máquina está instalada.
  3. Dispara a notificação push para os dispositivos móveis desse usuário via `IPushNotificationService` (implementado no Card 7).
  4. **Controle de Throttling (Anti-Spam):** O mesmo alerta só é disparado no celular a cada 2 minutos caso a máquina permaneça acima do limite, evitando sobrecarga no celular do operador.

---

## ✅ Critérios de Aceite Atendidos

| Critério do Card | Como Foi Cumprido |
| :--- | :--- |
| **Telemetry válida chega ao PostgreSQL** | Métricas válidas são mapeadas e salvas em `MachineMetrics`. |
| **Inválida é rejeitada** | Payloads sem `machineId`, máquinas inexistentes ou sem dados numéricos são descartados com log descritivo. |
| **Timestamp não vira métrica** | Identificado e tratado separadamente no cabeçalho temporal do registro. |
| **Reconexão e duplicata testadas** | Deduplicação por hash/id e reconexão em loop `while` ativo. |
| **Evento dispara fluxo push** | Criação de `MachineEvent` e chamada imediata ao `PushNotificationService`. |

---

## 🧪 Como Testar e Explicar para o Grupo

### Publicação no Broker MQTT (Exemplo Mosquitto)
Tópico: `lab/machine/metrics`

#### Cenário 1: Telemetria Normal (Persistência no Banco)
```json
{
  "messageId": "msg-001",
  "machineId": "SUA-MACHINE-GUID-AQUI",
  "timestamp": "2026-11-09T10:30:00Z",
  "temperature": 45.2,
  "rpm": 1500,
  "vibration": 1.2,
  "energyUsage": 12.5
}
```
*Resultado esperado:* Registro salvo em `MachineMetrics`. Nenhum alerta gerado.

#### Cenário 2: Violação de Limiar Crítico (Gera Evento + Push)
```json
{
  "messageId": "msg-002",
  "machineId": "SUA-MACHINE-GUID-AQUI",
  "timestamp": "2026-11-09T10:31:00Z",
  "temperature": 88.5,
  "vibration": 6.8
}
```
*Resultado esperado:*
1. Registro gravado em `MachineMetrics`.
2. Criação automática de dois eventos em `MachineEvents` (`ALERTA_TEMPERATURA` e `ALERTA_VIBRACAO`).
3. Disparo de push notification para os celulares cadastrados do operador com título `"⚠️ Alerta: NomeDaMaquina"`.

