# Black Core: Ascension — Auditoria Pré-Unity

Data: 2026-08-25
Base auditada: `blackcore-ascension-qa-pass.zip`

## Resultado executivo

O projeto está estruturalmente consistente para ser levado a um runner/Editor Unity, mas ainda **não está pronto para um build visual completo** porque o payload de arte gerado pelo pipeline UnityRO não está presente no ZIP.

### Estado por camada

- Servidor rAthena: **bom / previamente compilado no QA Pass**.
- Cliente C# / cenas / prefabs: **bom para primeira compilação Unity**, com algumas correções adicionais desta auditoria.
- UI serializada / eventos: **sem referências órfãs detectadas após correção**.
- Identidade visível: **sem termos legados detectados em textos serializados auditados**.
- Assets visuais completos: **bloqueador** — `Assets/_Generated/AddressablesAssets` ausente.
- Performance mobile: **precisa próxima sprint** — ainda existem carregamentos síncronos e material cloning.
- Produção pública: **não pronta** — login legado ainda usa credenciais sobre protocolo TCP do ecossistema RO.

## Validações automáticas desta auditoria

- 387 scripts C#.
- 9 cenas Unity.
- 49 prefabs.
- 30 shaders/shadergraphs.
- 1.143 scripts NPC no servidor.
- JSON inválido: 0.
- Scripts C# sem `.meta`: 0.
- GUIDs `.meta` duplicados: 0.
- Métodos persistentes de UI sem método correspondente (checagem aproximada): 0 após correção.
- Termos legados visíveis em `m_Text/m_text` de cenas/prefabs: 0 para o conjunto auditado.

## Bugs encontrados e corrigidos agora

### 1. Clique secundário em skill podia lançar exceção
`UISkill.OnRightClick()` ainda executava `NotImplementedException`. Foi transformado em comportamento seguro/no-op para não derrubar interação de hotbar/UI.

### 2. MeshEntityViewer tinha caminhos de cleanup não implementados
`FadeOut()` e `Init(SpriteData, Texture2D)` podiam lançar `NotImplementedException` em conteúdo 3D opcional. Agora o cleanup é seguro e o init redireciona para o caminho de mesh.

### 3. EscapeButton tinha callback serializado órfão
O prefab ainda apontava para `EscapeWindowController.ReturnToCharSelection`, classe/método que não existem mais. O callback persistente foi removido; `EscapeWindow` já adiciona listeners reais em runtime.

O menu de escape também recebeu labels PT-BR.

### 4. Kit inicial não cobria criação direta por Caminho
Depois da Character Creation Sprint, o personagem pode nascer diretamente como Guerreiro, Arcanista, Arqueiro, Devoto, Mercador ou Ladino. A recompensa de arma + 20 Poções Rubras existia apenas no fluxo Novice -> Maya.

Foi adicionado um fallback one-time em `OnPCLoginEvent` para garantir o mesmo kit aos seis Caminhos criados diretamente.

### 5. CI ainda baixava Addressables do UnityRO antigo
O workflow ainda fazia download de `settings.json` do host antigo `unityro.fra1.digitaloceanspaces.com` depois do build. Essa etapa foi removida para impedir que um build Black Core volte a depender da infraestrutura antiga.

### 6. Preflight automático criado
Novo `tools/blackcore_preflight.py` verifica sem Unity:
- JSON;
- `.meta` de C#;
- GUIDs duplicados;
- termos legados visíveis serializados;
- callbacks persistentes de UI;
- presença do payload de Addressables gerado.

O workflow de testes passa a executar esse preflight antes do Unity Test Runner.

## Bloqueador visual: assets gerados ausentes

O projeto mantém catálogos/grupos Addressables enormes, mas o ZIP não contém a biblioteca correspondente em `Assets/_Generated/AddressablesAssets`.

Referências aproximadas presentes nos grupos:
- Sprites: 85.420 entradas.
- Textures: 46.082 entradas.
- Models: 34.748 entradas.
- Palettes: 2.578 entradas.
- Effects: 1.657 entradas.
- Wav: 2.709 entradas.
- BGM: 186 entradas.
- Maps: 48 entradas.

Quase todos esses GUIDs não têm o asset correspondente no ZIP atual. Isso é coerente com o pipeline original do UnityRO, que gera `_Generated` a partir de uma fonte GRF.

**Consequência:** abrir o projeto no Unity pode compilar o código, mas o jogo completo não terá sprites/mapas/modelos/texturas suficientes até esse payload ser gerado/substituído.

Para Black Core, o caminho recomendado é construir um **asset pack próprio/licenciado** e alimentar o mesmo pipeline de Addressables, em vez de depender de arte proprietária antiga.

## Riscos de performance ainda presentes

Foram encontrados aproximadamente:
- 22 usos de `WaitForCompletion()` em runtime/editor shared code;
- 41 acessos/atribuições via `Renderer.material` em scripts runtime (podem clonar materiais);
- 52 usos de `FindObjectOfType/FindObjectsOfType` em scripts do cliente.

Prioridade maior: `SpriteEntityViewer` ainda carrega body/head/weapon/palette de forma síncrona durante criação/troca de entidades. Em Android isso pode causar hitch/stutter em cidades ou troca de equipamento.

## Conteúdo legado do servidor ainda ativo

O `npc/re/scripts_main.conf` continua importando o conjunto oficial inteiro de scripts rAthena e depois os scripts Black Core. A camada de apresentação esconde muitos nomes, mas scripts/quests/eventos oficiais ainda podem vazar conteúdo antigo em áreas não curadas.

Próxima aplicação recomendada: criar um **Black Core Content Profile** separado, com whitelist de funções/mapflags/warps necessários + conteúdo próprio, em vez de carregar todo o pacote oficial no servidor de produção.

## Segurança / produção

A autenticação do cliente ainda usa o protocolo legado de login do ecossistema RO. Para teste fechado isso pode ser tolerado em ambiente controlado, mas não deve ser tratado como autenticação moderna segura para lançamento público.

Antes de produção: gateway HTTPS/token, túnel seguro ou camada de autenticação própria.

## Próximas aplicações antes do primeiro APK

1. **Asset Pack / Addressables Black Core** — prioridade máxima.
2. **Async Entity Assets** — remover `WaitForCompletion()` do hot path de personagens/monstros/equipamentos.
3. **Black Core Content Profile** — isolar scripts oficiais antigos e reduzir vazamento de identidade/conteúdo.
4. **Android CI remoto** — habilitar `Android` no GameCI quando o asset pack existir e configurar keystore/Unity license.
5. **UI Safe Area audit automatizado** — presets 16:9, 19.5:9, 20:9, tablets e notch.
6. **Auth de produção** — separar login público do protocolo legado.
7. **Material/cache pass** — reduzir `renderer.material` e clones de material, especialmente mapa/sprites/efeitos.
8. **Quest/content expansion** — campanha, classes, instâncias e world events depois da fundação visual/Android estar fechada.

## O que só o Unity/aparelho pode confirmar

- compilação C# real com assemblies/packages Unity;
- referências Addressables após gerar o payload;
- layout renderizado e clipping;
- safe area/notch real;
- shader/URP/Bloom/fog na GPU;
- FPS, memória e GC em Android;
- touch, multitouch e joystick;
- áudio;
- build APK/AAB;
- login -> criação -> seleção -> mapa -> combate -> quest end-to-end.
