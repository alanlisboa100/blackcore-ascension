# Black Core: Ascension — Quest & Power UI Sprint

## Missões reais
- Implementados pacotes rAthena de quest no cliente:
  - `ZC_ALL_QUEST_LIST3 (0x09F8)`
  - `ZC_ADD_QUEST_EX (0x09F9)`
  - `ZC_HUNTING_QUEST_INFO (0x08FE)`
  - `ZC_DEL_QUEST (0x02B4)`
- Novo `BlackCoreQuestJournal` persistente, alimentado pelo estado real do map-server.
- Tracker no HUD mostra até duas missões ativas e progresso real.
- Diário do Núcleo abre em painel próprio e lista objetivos das quests.
- Títulos/lore próprios para `Primeiro Pulso` e `Eco do Abismo`.
- Nomes de monstros já customizados pelo servidor são preservados, evitando dupla renomeação.

## Skills
- Janela renomeada para `Árvore de Poder`.
- `Skill Points` virou `Pontos de Poder`.
- Skin Black Core aplicada à grade e slots sem alterar drag/use/upgrade.

## Inventário e equipamento
- Passe adicional de acabamento em células, slots, tabs e fundos.
- Ícones e funcionalidade real permanecem intactos.

## Validação
- Checagem estática de chaves/arquivos C# passou.
- Testes EditMode adicionados para parser de lista/progresso de quests.
- A compilação final do cliente ainda precisa ser validada dentro do Unity Editor 2021.3/URP 12.
