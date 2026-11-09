# Contrato de API — Consultas de Dashboard e Agregações

## 1. Resumo da Planta
- **Endpoint:** `GET /api/dashboard/plants/{plantId}/summary`
- **Autenticação:** Bearer JWT Obrigatório
- **Objetivo:** Fornecer indicadores de status das máquinas da planta e médias das últimas 24h.

### Resposta de Sucesso (HTTP 200)
```json
{
  "plantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "plantName": "Planta Principal",
  "totalMachines": 10,
  "activeMachines": 8,
  "alertMachines": 2,
  "offlineMachines": 0,
  "openEventsCount": 2,
  "averageTemperature": 58.7,
  "temperatureUnit": "°C",
  "averageVibration": 1.85,
  "vibrationUnit": "mm/s",
  "totalEnergyAccumulated": 320.40,
  "energyUnit": "kWh",
  "averagePower": 13.35,
  "powerUnit": "kW"
}
```

---

## 2. Série Temporal de Métricas Agregadas
- **Endpoint:** `GET /api/dashboard/metrics/aggregated`
- **Autenticação:** Bearer JWT Obrigatório
- **Parâmetros de Consulta (Query Params):**
  - `plantId` (UUID, opcional): Filtrar todas as máquinas de uma planta.
  - `machineId` (UUID, opcional): Filtrar uma máquina específica.
  - `startDate` (ISO 8601, opcional): Data inicial (default: últimas 24 horas).
  - `endDate` (ISO 8601, opcional): Data final (default: momento atual).
  - `interval` (string, opcional): `"hour"` (padrão) ou `"day"`.

### Resposta de Sucesso (HTTP 200)
```json
{
  "plantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "machineId": null,
  "startDate": "2026-11-08T00:00:00Z",
  "endDate": "2026-11-09T00:00:00Z",
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
      "timestamp": "2026-11-08T01:00:00Z",
      "timeLabel": "08/11 01:00",
      "avgTemperature": 52.3,
      "maxTemperature": 58.0,
      "avgVibration": 1.20,
      "maxVibration": 1.45,
      "avgRpm": 1200,
      "totalEnergyUsage": 11.5,
      "averagePower": 5.75,
      "sampleCount": 60
    }
  ]
}
```

