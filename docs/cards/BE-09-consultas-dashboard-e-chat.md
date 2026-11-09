# [S4][BE-09] Consultas de Dashboard e Revisão do Chat com IA

## 📌 Visão Geral e Objetivo
Implementação de consultas agregadas para o dashboard industrial web/mobile e formalização da documentação do serviço de assistente inteligente (Chatbot IA). Esse módulo elimina a sobrecarga no frontend de ter que baixar milhares de linhas de telemetria bruta para gerar gráficos, entregando dados consolidados por período (hora ou dia) com contratos tipados e unidades industriais bem definidas.

---

## 🛠️ O Que Foi Implementado

### 1. DTOs de Dashboard Tipados
- [`DashboardPlantSummaryDto`](../../DTOs/Dashboard/DashboardPlantSummaryDto.cs): Resumo instantâneo da planta contendo:
  - Total de máquinas, máquinas ativas, em alerta e offline.
  - Total de alertas/eventos em aberto no parque.
  - Indicadores consolidados das últimas 24h: temperatura média (°C), vibração média (mm/s), energia total acumulada (kWh) e potência média (kW).
- [`MetricTimeSeriesPointDto`](../../DTOs/Dashboard/MetricTimeSeriesPointDto.cs): Ponto temporal para alimentar gráficos (ApexCharts, Chart.js, etc.) contendo médias, picos e rótulo temporal amigável (`TimeLabel`, ex: `"09/11 14:00"`).
- [`DashboardTimeSeriesResponseDto`](../../DTOs/Dashboard/DashboardTimeSeriesResponseDto.cs): Resposta de série temporal contendo a lista de pontos e um mapa explícito de unidades de medição.

### 2. Distinção de Unidades Industriais no Contrato
Para cumprir rigorosamente o critério de aceite, o contrato devolve as unidades tipadas:
```json
"units": {
  "temperature": "°C",
  "vibration": "mm/s",
  "rpm": "RPM",
  "energyUsage": "kWh",
  "power": "kW"
}
```
* **Consumo de Energia:** Somatório acumulado no intervalo em Quilowatt-hora (**kWh**).
* **Potência Média:** Demanda de potência instantânea em Quilowatt (**kW**).

### 3. Camada de Serviço e Segurança (`DashboardService.cs`)
- **Isolamento de Tenant (Usuário):** O usuário só consegue visualizar resumos e métricas de plantas e máquinas que pertençam à sua própria conta (`p.UserId == userId`). Nenhuma métrica vaza entre usuários.
- **Agrupamento Eficiente:** O agrupamento de dados é calculado no backend agrupando por hora (`hour`) ou por dia (`day`), poupando tráfego de rede e consumo de bateria no app mobile.

### 4. Controller REST (`DashboardController.cs`)
- `GET /api/dashboard/plants/{plantId}/summary`: Resumo geral e saúde dos equipamentos da planta.
- `GET /api/dashboard/metrics/aggregated`: Série temporal filtrável por `plantId`, `machineId`, `startDate`, `endDate` e `interval` (`hour` ou `day`).

### 5. Revisão do Módulo de Chat com IA ([`ChatController.cs`](../../Controllers/ChatController.cs) e [`ChatService.cs`](../../Services/ChatService.cs))
- **Status do Chat:** Mantido e preservado no escopo do projeto integrador.
- **Integração Externa:** Google Gemini API (`https://generativelanguage.googleapis.com`).
- **Configuração Necessária:** Chave de API no `appsettings.json` ou variável de ambiente:
  ```json
  "Gemini": {
    "ApiKey": "SUA_CHAVE_GEMINI_AQUI"
  }
  ```
- **Privacidade e Dados Enviados:** O assistente recebe em tempo de execução apenas a lista de plantas, nomes de máquinas, modelos, status e eventos **em aberto** pertencentes ao usuário logado. Nenhuma senha, token ou dado de outros usuários é transmitido ao Gemini.
- **Diretiva de Sistema:** O Gemini é estritamente instruído a responder com base apenas nas informações reais fornecidas, sem inventar sensores ou dados fictícios.

---

## ✅ Critérios de Aceite Atendidos

| Critério do Card | Como Foi Cumprido |
| :--- | :--- |
| **Web/mobile recebem métricas consistentes sem buscar todos os usuários** | `DashboardService` agrega as métricas e filtra unicamente pelo `UserId` autenticado. |
| **Consumo e potência têm unidades distintas** | Energia retornada em `kWh` e potência média estimada em `kW`. |
| **Chat funciona ou sai explicitamente do escopo** | Chat mantido, contextualizado por usuário e totalmente documentado. |

---

## 🧪 Como Testar e Explicar para o Grupo

### 1. Consultar Resumo da Planta
- **Endpoint:** `GET /api/dashboard/plants/{plantId}/summary`
- **Header:** `Authorization: Bearer <SEU_TOKEN_JWT>`
- **Exemplo de Resposta:**
```json
{
  "plantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "plantName": "Planta Principal",
  "totalMachines": 5,
  "activeMachines": 4,
  "alertMachines": 1,
  "offlineMachines": 0,
  "openEventsCount": 1,
  "averageTemperature": 62.4,
  "temperatureUnit": "°C",
  "averageVibration": 2.15,
  "vibrationUnit": "mm/s",
  "totalEnergyAccumulated": 184.50,
  "energyUnit": "kWh",
  "averagePower": 7.68,
  "powerUnit": "kW"
}
```

### 2. Consultar Série Temporal para Gráficos
- **Endpoint:** `GET /api/dashboard/metrics/aggregated?plantId={plantId}&interval=hour`
- **Header:** `Authorization: Bearer <SEU_TOKEN_JWT>`
- **Exemplo de Resposta:**
```json
{
  "plantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "machineId": null,
  "startDate": "2026-11-08T08:00:00Z",
  "endDate": "2026-11-09T08:00:00Z",
  "interval": "hour",
  "units": {
    "temperature": "°C",
    "vibration": "mm/s",
    "rpm": "RPM",
    "energyUsage": "kWh",
    "power": "kW"
  },
  "series": [
    {
      "timestamp": "2026-11-09T08:00:00Z",
      "timeLabel": "09/11 08:00",
      "avgTemperature": 58.2,
      "maxTemperature": 64.0,
      "avgVibration": 1.45,
      "maxVibration": 1.90,
      "avgRpm": 1800,
      "totalEnergyUsage": 14.2,
      "averagePower": 7.1,
      "sampleCount": 12
    }
  ]
}
```

