# ADR 01: Decisão Arquitetural sobre Manutenção do Ecossistema .NET 9 vs. Migração para Spring Boot

* **Status:** Aprovado / Decidido
* **Data:** 09/11/2026
* **Decisores:** Equipe de Engenharia de Software - LabControl 4.0
* **Referência:** Issue [#67 (BE-01)](https://github.com/labControl4-0/labcontrol-api/issues/67)

---

## 1. Contexto do Problema
No início do quarto semestre do Projeto Integrador IV, surgiu a proposta no Card `[BE-01]` de avaliar e potencialmente migrar todo o backend da aplicação de C# / .NET para Java / Spring Boot.

O backend existente em .NET 9 já contava com:
- Persistência estruturada em PostgreSQL via Entity Framework Core com Migrations versionadas.
- Autenticação e autorização via JWT Bearer Token e hash de senha com BCrypt.
- Ingestão contínua de telemetria IoT via protocolo MQTT (`MQTTnet`) em background.
- Integração com inteligência artificial generativa (Google Gemini API).

---

## 2. Análise Técnica e Riscos Levantados
A migração total para Spring Boot apresentaria os seguintes riscos e custos:
1. **Risco de Quebra de Contratos Existentes:** Os clientes existentes (Web React/Next.js e Mobile React Native) já consomem os contratos REST atuais. Reescrever o backend traria alto risco de divergência de tipos (ex: formato de serialização de polígonos JSON de setores, tratamento de precisão decimal de escalas e enums).
2. **Custo de Oportunidade e Reescrita Desnecessária:** Em vez de focar nas novas funcionalidades do semestre (notificações push no celular, sincronização offline idempotente e dashboards com agregações temporais), o esforço de engenharia seria drenado em recriar funcionalidades que já estavam prontas e maduras.
3. **Desempenho e Eficiência do .NET 9:** O .NET 9 apresenta benchmarks de altíssimo throughput de I/O assíncrono e baixo consumo de memória, especialmente benéfico para conexões contínuas de MQTT e requisições concorrentes.

---

## 3. Decisão Arquitetural Adotada
**Decidiu-se manter o backend integralmente em C# / .NET 9.**

Nenhuma tecnologia foi substituída. Em vez disso, todo o escopo de novas regras de negócio do semestre foi implementado diretamente no backend .NET existente:
- **[BE-07]** Implementação de registro de dispositivos móveis e disparo de notificações push via Expo Push Service.
- **[BE-08]** Aprimoramento do motor de ingestão MQTT com reconexão contínua, deduplicação em memória, verificação de limiares industriais e gatilho automático de alertas + push.
- **[BE-09]** Desenvolvimento de endpoints com agregações analíticas (médias, consumo em kWh e potência em kW) e consolidação do Chat inteligente com IA.
- **[BE-06]** Criação do subsistema de sincronização atômica idempotente com detecção de conflitos para o aplicativo mobile offline.

---

## 4. Consequências e Resultados
- **Positivas:**
  - 100% de paridade e estabilidade com o aplicativo web e mobile.
  - Zero tempo de inatividade ou necessidade de migrações complexas de esquema entre EF Core e Hibernate/JPA.
  - Conclusão de 100% dos requisitos de negócio com menor prazo e maior robustez.
  - Documentação unificada dos contratos e rastreabilidade total de cada card.

