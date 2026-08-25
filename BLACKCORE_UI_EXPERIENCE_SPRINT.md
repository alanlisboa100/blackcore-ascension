# Black Core UI Experience Sprint

## Seleção de personagem
- Cabeçalho Black Core: **Escolha seu Viajante**.
- Cada personagem passa a exibir nome, Caminho, nível e região.
- Nome de classe usa `BlackCoreLoreService` em vez do nome legado cru.
- Labels comuns são traduzidas em runtime.

## Seleção de servidor
- Cabeçalho **Selecione o Núcleo**.
- Botão de cancelar agora volta corretamente para o login.
- Skin Black Core aplicada sem trocar a lógica de conexão.

## NPCs e missões
- Janela agora recebe identidade **Transmissão do Núcleo**.
- Novo `BlackCoreQuestPresentation` formata vocabulário comum de missões.
- Destaques visuais para OBJETIVO, PROGRESSO e RECOMPENSA via rich text.
- Diálogo continua vindo do servidor; não existe quest falsa/local.

## Inventário
- Identidade **Mochila do Viajante**.
- Botões/textos recebem palette Black Core em runtime.
- A grade, tabs e os itens reais continuam usando o sistema existente.

## Equipamentos
- Identidade **Arsenal**.
- Preview e slots continuam funcionais.
- Skin visual e tradução aplicada sem alterar equip/unequip.

## Sweep de identidade
A camada de apresentação também passa a interceptar termos visíveis como:
- Ragnarok -> Black Core
- UnityRO -> Black Core
- rAthena -> Núcleo do Servidor

As assemblies, namespaces e identificadores técnicos continuam intactos para não quebrar serialização do Unity.
