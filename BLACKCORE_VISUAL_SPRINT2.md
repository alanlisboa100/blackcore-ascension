# Black Core Visual Sprint 2

## Aplicado nesta sprint

### Login / onboarding
- Rebranding runtime do painel de login para **Black Core: Ascension**.
- Tradução de labels do login para PT-BR (`Usuário`, `Senha`, `Entrar`, `Sair`).
- Header e footer dinâmicos com identidade Black Core.
- Estilização dos campos de entrada e botão principal.
- `OnExitClicked()` agora encerra o app corretamente.

### Casas, árvores e mapas
- Novo `BlackCoreWorldDetailPass` para polimento de malhas depois que o mapa inteiro é carregado.
- Renomeia o objeto-raiz do mapa usando `BlackCoreLoreService.ResolveMapName()`.
- Aplica heurísticas extras em:
  - vegetação
  - casas / telhados / paredes / janelas
  - chão / estrada / pedra / tijolo
  - props mágicos (`portal`, `crystal`, `torch`, etc.)
- Reforça sombras e variação visual sem quebrar assets legados.

### Monstros / sprites
- Novo `BlackCoreSpritePolish` para dar acabamento de apresentação em monstros e armas.
- Chefes recebem destaque visual sutil.
- Armas de jogador recebem leve acento de cor.

### Sweep de identidade
- Expansão do filtro de termos legados na camada de apresentação:
  - `Rune-Midgard`
  - `Midgard`
  - `Adventurer`
  - `Kafra`
  - `Emperium`
  - `MVP`
- Objetivo: reduzir a chance de termos antigos aparecerem na UI.

## Arquivos principais tocados
- `Assets/Scenes/Login/LoginController.cs`
- `Assets/Scripts/Renderer/MapRenderer.cs`
- `Assets/Scripts/Renderer/Entities/SpriteEntityViewer.cs`
- `Assets/Scripts/Visual/BlackCoreEnvironmentPolish.cs`
- `Assets/Scripts/Visual/BlackCoreWorldDetailPass.cs`
- `Assets/Scripts/Visual/BlackCoreSpritePolish.cs`
- `Assets/Scripts/Identity/BlackCoreLoreService.cs`
