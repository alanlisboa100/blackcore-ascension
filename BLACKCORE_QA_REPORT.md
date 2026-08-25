# Black Core: Ascension — QA Pass

## O que foi validado neste ambiente

### Servidor
- `login-server`: compilou e linkou com sucesso.
- `char-server`: compilou e linkou com sucesso.
- `map-server`: compilou e linkou com sucesso.
- `tools/blackcore_identity_sync.py`: `py_compile` passou.
- `deploy/docker/entrypoint.sh`: sintaxe Bash passou.

### Cliente — integridade estática
- 9 cenas Unity auditadas.
- 49 prefabs auditados.
- 387 scripts C# presentes.
- Nenhuma cena/prefab contém `m_Script: {fileID: 0}`.
- Nenhum texto serializado visível encontrado contendo `Ragnarok`, `UnityRO`, `rAthena`, `Prontera`, `Geffen`, `Payon`, `Morroc`, `Zeny`, `Kafra` ou `Emperium`.
- Todos os scripts C# agora têm `.meta` versionado.

## Bugs encontrados e corrigidos

1. **ZC_ADD_QUEST_EX estava desalinhado**
   - O parser pulava bytes entre `active` e os timestamps.
   - Agora segue o layout real do rAthena: `questID + active + startTime + expireTime + count`.
   - O tamanho de cada objetivo foi corrigido para 42 bytes, permitindo ler os 3 objetivos do pacote fixo de 143 bytes.
   - Teste de regressão adicionado para tempos + três objetivos.

2. **Conflito do HUD de missões no mobile**
   - O botão `MISSÕES`/tracker ocupava a mesma faixa de `BAG`/`SKL`.
   - No layout mobile o diário agora desce para uma faixa separada.

3. **Dicionário de identidade podia falhar em runtime**
   - `DialogueAliases` usa `StringComparer.OrdinalIgnoreCase`, mas possuía as chaves duplicadas `Zeny` e `zeny`.
   - A duplicata foi removida para evitar `ArgumentException` na inicialização estática.

4. **Scripts novos sem `.meta`**
   - 16 scripts de sprints anteriores estavam sem `.meta` versionado.
   - Foram adicionados `.meta` estáveis para evitar GUIDs novos e referências inconsistentes ao importar o projeto.

## Limite da validação

Não há Unity Editor 2021.3 disponível neste ambiente. Portanto, ainda não foi possível validar visualmente frame a frame:
- clipping/overflow em todas as resoluções;
- fontes e alinhamentos finais renderizados;
- efeitos URP na GPU real;
- toque, notch e safe area em aparelho físico;
- build APK final;
- compile de C# dentro do pipeline do Unity.

A próxima validação definitiva precisa ser um build Unity/Android em runner x86_64 e um smoke test no aparelho.
