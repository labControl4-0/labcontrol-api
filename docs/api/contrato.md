# Contrato Geral da API — LabControl 4.0

* **Tecnologia:** C# / .NET 9.0 Web API
* **Banco de Dados:** PostgreSQL 15 via EF Core
* **Autenticação:** Bearer JWT

---

## 📌 Inventário Consolidado de Endpoints

### 🔐 1. Autenticação e Conta (`/api/auth`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Cadastro de usuário |
| `POST` | `/api/auth/login` | Login com e-mail/senha, retorna JWT |
| `GET` | `/api/auth/profile` | Dados do usuário autenticado |
| `PUT` | `/api/auth/profile` | Atualização de perfil |
| `POST` | `/api/auth/forgot-password`| Solicitação de redefinição de senha |
| `POST` | `/api/auth/reset-password` | Confirmação da nova senha |

### 👥 2. Gestão de Usuários (`/api/users`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/users` | Listagem de usuários (Admin/Manager) |
| `GET` | `/api/users/{id}` | Busca por ID |
| `PUT` | `/api/users/{id}` | Edição de usuário |
| `DELETE` | `/api/users/{id}` | Inativação/remoção |

### 🏭 3. Plantas Industriais (`/api/plants`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/plants` | Lista plantas do usuário logado |
| `GET` | `/api/plants/{id}` | Busca planta completa (com setores e máquinas) |
| `POST` | `/api/plants` | Criação de planta com dimensões reais e escala |
| `PUT` | `/api/plants/{id}` | Atualização de planta |
| `DELETE` | `/api/plants/{id}` | Remoção de planta |

### 📐 4. Setores da Fábrica (`/api/sector`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/sector/plant/{plantId}` | Lista setores de uma planta |
| `GET` | `/api/sector/{id}` | Detalhes do setor |
| `POST` | `/api/sector` | Criação de setor com polígono e cálculo de área ($m^2$) |
| `PUT` | `/api/sector/{id}` | Atualização |
| `DELETE` | `/api/sector/{id}` | Remoção |

### ⚙️ 5. Máquinas (`/api/machines`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/machines/plant/{plantId}` | Lista máquinas de uma planta |
| `GET` | `/api/machines/{id}` | Detalhes da máquina com últimas métricas e eventos |
| `POST` | `/api/machines` | Cadastro de máquina (posição 2D X/Y, setor e status) |
| `PUT` | `/api/machines/{id}` | Atualização de máquina |
| `DELETE` | `/api/machines/{id}` | Exclusão de máquina |

### 📱 6. Dispositivos Móveis e Push (`/api/devices`) - [BE-07]
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/devices` | Lista dispositivos móveis do usuário autenticado |
| `POST` | `/api/devices` | Registra/atualiza Expo Push Token do aparelho |
| `DELETE` | `/api/devices?token=...` | Desvincula e inativa token |
| `POST` | `/api/devices/test-push` | Dispara notificação push de teste para o celular |

### 📊 7. Dashboard e Indicadores (`/api/dashboard`) - [BE-09]
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/dashboard/plants/{plantId}/summary` | Resumo consolidado da planta (ativos, alertas, médias 24h) |
| `GET` | `/api/dashboard/metrics/aggregated` | Série temporal agregada por hora ou dia para gráficos |

### 🔄 8. Sincronização Offline Idempotente (`/api/sync`) - [BE-06]
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/sync` | Processamento atômico de lote offline com idempotência e conflitos |

### 🚨 9. Eventos e Alertas (`/api/machineevents`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/machineevents/machine/{machineId}` | Histórico de eventos da máquina |
| `POST` | `/api/machineevents` | Criação de evento manual |
| `PUT` | `/api/machineevents/{id}/resolve` | Resolução de alerta/evento |

### 📡 10. Métricas de Telemetria (`/api/machinemetrics` e `/api/iotmetrics`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/machinemetrics/machine/{machineId}` | Histórico de leituras |
| `POST` | `/api/machinemetrics` | Ingestão manual de métricas |
| `POST` | `/api/iotmetrics` | Ingestão simplificada |

### 🤖 11. Assistente Inteligente IA (`/api/chat`)
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/chat` | Chat contextualizado com a API do Google Gemini |

