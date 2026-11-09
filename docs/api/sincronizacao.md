# Contrato de API — Sincronização Idempotente Offline

## Especificação Técnica do Endpoint `POST /api/sync`

### 1. Propósito
Permitir que clientes móveis enviem lotes de dados operacionais coletados ou modificados em modo offline (sem conectividade com a internet), garantindo transações atômicas, resolução/detecção de conflitos e total idempotência através de identificadores únicos por operação (`ClientOperationId`).

---

### 2. Cabeçalhos Obrigatórios
| Cabeçalho | Tipo | Descrição |
| :--- | :--- | :--- |
| `Authorization` | `string` | `Bearer <JWT_TOKEN>` do operador autenticado |
| `Content-Type` | `string` | `application/json` |

---

### 3. Modelo de Requisição (`SyncBatchRequestDto`)

| Campo | Tipo | Obrigatório | Descrição |
| :--- | :--- | :--- | :--- |
| `clientId` | `UUID` | Sim | Identificador do dispositivo do operador |
| `operations` | `Array` | Sim | Lista de operações realizadas em modo offline |

#### Objeto do Item da Operação (`SyncOperationItemDto`):
| Campo | Tipo | Obrigatório | Descrição |
| :--- | :--- | :--- | :--- |
| `clientOperationId` | `UUID` | Sim | UUID único gerado pelo cliente para esta operação |
| `entityType` | `string` | Sim | Tipo de entidade (`MachineEvent`, `Machine`, `Plant`) |
| `action` | `string` | Sim | `CREATE`, `UPDATE`, `RESOLVE`, `DELETE` |
| `entityId` | `UUID` | Opcional | ID do recurso (obrigatório para UPDATE e RESOLVE) |
| `clientTimestamp` | `ISO 8601` | Sim | Horário em que a ação ocorreu offline no aparelho |
| `data` | `JSON Object`| Opcional | Carga de dados (campos a criar ou atualizar) |

---

### 4. Modelo de Resposta (`SyncBatchResponseDto`)

| Campo | Tipo | Descrição |
| :--- | :--- | :--- |
| `serverTimestamp` | `ISO 8601` | Horário oficial do servidor no momento do processamento |
| `totalProcessed` | `int` | Quantidade total de operações recebidas no lote |
| `successCount` | `int` | Quantidade de operações aplicadas com sucesso |
| `conflictCount` | `int` | Quantidade de operações que entraram em conflito de versão |
| `errorCount` | `int` | Quantidade de operações com erro de validação ou permissão |
| `results` | `Array` | Resultado individual por operação |

#### Objeto de Resultado Individual (`SyncOperationResultDto`):
| Campo | Tipo | Descrição |
| :--- | :--- | :--- |
| `clientOperationId` | `UUID` | UUID correspondente enviado pelo cliente |
| `status` | `string` | `SUCCESS`, `ALREADY_PROCESSED`, `CONFLICT` ou `ERROR` |
| `message` | `string` | Descrição do resultado da operação |
| `entityId` | `UUID?` | ID do registro criado ou modificado no servidor |
| `currentServerState`| `Object?` | Estado real no servidor retornado caso `status == "CONFLICT"` |

---

### 5. Tabela de Estados de Sincronização

| Status | Significado | Ação do Cliente Mobile |
| :--- | :--- | :--- |
| **`SUCCESS`** | Operação aplicada com sucesso no PostgreSQL | Marcar registro local como sincronizado |
| **`ALREADY_PROCESSED`** | Operação já havia sido aplicada anteriormente | Marcar como sincronizado (idempotência atendida) |
| **`CONFLICT`** | O recurso foi alterado no servidor após o timestamp do celular | Apresentar tela de conflito ou aplicar estratégia de merge com `currentServerState` |
| **`ERROR`** | Dados inválidos ou falta de permissão | Manter em fila de erro para inspeção |

