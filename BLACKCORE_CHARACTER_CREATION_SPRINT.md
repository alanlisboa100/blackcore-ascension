# Black Core: Ascension — Character Creation Sprint

## Objetivo
Reformar a criação de personagem sem usar uma imagem estática por cima do sistema real.

## O que mudou
- Preview continua sendo o **Entity/Sprite real** do cliente.
- Seis caminhos iniciais funcionais:
  - Combatente → Guerreiro (job 1)
  - Atirador → Arqueiro (job 3)
  - Arcano → Arcanista (job 2)
  - Devoto → Devoto (job 4)
  - Sombra → Ladino (job 6)
  - Artesão → Mercador (job 5)
- Selecionar um caminho muda o job/sprite real do preview.
- `CH.MAKE_CHAR2` envia o `StartJob` selecionado.
- char-server foi alterado para aceitar somente as seis primeiras classes Black Core adicionais, além dos jobs originais permitidos.
- Cabelo, sexo e nome continuam usando o fluxo real de criação.
- Nome recebe sugestão Black Core automática.
- Labels antigos da criação foram traduzidos/rebatizados.
- Painel lateral runtime com descrição dos caminhos e feedback visual de seleção.
- Doram foi removido da apresentação principal desta experiência para evitar uma raça legado aparecendo como escolha central.

## Validação
- `make char -j2`: PASS
- char-server compilado e linkado com a nova regra de jobs iniciais.
- Unity Editor não está disponível neste ambiente; a renderização/layout da UI precisa de teste visual no primeiro build Unity.
