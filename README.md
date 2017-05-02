# Idiomas | Languages | 言語
- [Português](#Índice)
- [English](#index)
- [日本語](#目次)

---

# Índice
1. [Sobre o jogo](#sobre-o-jogo)
2. [Sobre a IA](#sobre-a-ia)
3. [Resultados](#resultados)
4. [Vídeo](#vídeo)

# Sobre o jogo
Para a explicação completa, [clique aqui](https://github.com/mateus-etto/Monografia-IAJogoRPG/releases) para ler a monografia.

Neste projeto foi criado uma Inteligência Artificial que controla personagens de um jogo RPG em turnos.
O objetivo é que os personagens sejam capazes de derrotar o time inimigo o quanto antes.

## Atributos
Cada personagem do jogo foi implementado com os seguintes atributos:

**HP (Pontos de Vida):**
É o quanto de dano o personagem pode receber antes de ser incapacitado.

**MP (Pontos de Magia):**
É consumido ao utilizar habilidades.

**ATK (Ataque):**
Um valor alto de ataque permite infligir maiores danos físicos.

**DEF (Defesa):**
Um valor alto de defesa reduz os danos físicos que serão infligidos no HP.

**MAG (Magia):**
Equivalente ao ATK, mas infligindo danos mágicos.

**RES (Resistência):**
Equivalente ao DEF, mas reduzindo danos mágicos que serão infligidos no HP.

**SPD (Velocidade):**
Velocidade do personagem, e valores altos o permite executar mais comandos em menos tempo.

## Tipos de danos
Existem dois tipos de danos, o físico e o mágico.
Danos mágicos são elementais, sendo:
água, fogo, terra e vento.
Cada personagem pode ser fraco, neutro ou forte em relação em algum tipo de dano.

## Cálculos de danos
O dano físico é calculado pela seguinte fórmula:

![FormulaDanoFisico](images/pt-BR/DanoFisico.png)

O dano mágico é calculado pela seguinte fórmula:

![FormulaDanoMagico](images/pt-BR/DanoMagico.png)

## Comandos
Quando um personagem recebe um turno,
ele pode escolher um entre 4 comandos básicos:
Atacar, Defender, Usar Habilidade e Usar Item.

**Ataque** é o comando ofensivo mais simples de todos.
Não consome MP, e causa dano físico razoavelmente baixo.

**Defesa** é um comando defensivo que reduz o dano a ser recebido.
Após todos os cálculos de dano, é verificado se o personagem está defendendo.
Se estiver, aquele dano é reduzido em 50%.

**Habilidade** é um comando que possui 3 subcomandos,
cada um deles representando uma habilidade diferente:
- **Habilidade Fraca** é uma habilidade que pode causar tanto dano físico quanto mágico,
consome 50 MP, e após os cálculos de dano, o valor de dano é aumentado em 50%.
- **Habilidade Forte** é semelhante a fraca, consome 110 MP, e causa um aumento de dano de 100%.
- **Habilidade de Cura** é uma habilidade que permite recuperar o HP de algum personagem.
Por ser uma habilidade de suporte, a RES do alvo é ignorada, sendo aplicada uma cura equivalente a 50% do MAG do personagem que está aplicando a cura.

**Item** é um comando que permite o uso de itens consumíveis durante a batalha.
Existem 6 itens diferentes, um é Poção e os outros 5 são ofensivos, um para cada tipo de dano existente no jogo.
A poção tem valor constante de cura, e os outros tem um valor de "ataque" semelhante a ATK ou MAG, que é independente do personagem que irá usá-lo.

## Atributos dos personagens
| Parâmetro  | Tanker | Guerreiro | Mago |
| :---: | :---: | :---: | :---: |
| **HP Max** | 750 | 500 | 400 |
| **MP Max** | 350 | 550 | 800 |
| **ATK** | 250 | 475 | 200 |
| **DEF** | 500 | 425 | 215 |
| **MAG** | 200 | 250 | 510 |
| **RES** | 400 | 210 | 450 |
| **SPD** | 82 | 98 | 79 |
| **Defesa Física** | Alta | Alta | Normal |
| **Defesa de Água** | Normal | Baixa | Alta |
| **Defesa de Fogo** | Alta | Normal | Normal |
| **Defesa de Terra** | Normal | Normal | Alta |
| **Defesa de Vento** | Alta | Normal | Alta |
| **Hab. Fraca** | Terra | Física | Alta |
| **Hab. Forte** | Física | Fogo | Alta |

# Sobre a IA
A IA foi feita com conceitos de Redes Neurais Artificiais e Algoritmo Genético.
Durante o jogo, cada equipe possui uma Inteligência Artificial que simboliza o jogador que os controla.

## Rede neural implementada
A Rede Neural Artificial implementada possui 27 neurônios na camada de entrada e 12 neurônios na camada de saída.
A RNA é responsável pela decisão de quem será o alvo do personagem que recebeu o turno.

Nos neurônios da camada de entrada, são usados os atributos ofensivos do atacante,
todos os atributos do possível alvo
e algumas informações de contexto, como se o alvo é inimigo e se ele está se defendendo.
A RNA é executada usando atributos do personagem que recebeu turno e dos possíveis alvos, um de cada vez.

![RedeNeural](images/pt-BR/NeuralNetwork.png)

Na camada de saída, tem-se o resultado da probabilidade daquele personagem se tornar alvo e a probabilidade de cada comando.

## Algoritmo Genético
O algoritmo genético utilizado para treinar a RNA possui 40 indivíduos,
sendo que cada um possui a matriz de pesos da RNA de uma equipe.
O ciclo do algorítmo desenvolvido é:

![AlgoritmoGenetico](images/pt-BR/GeneticAlgorithmFlow.png)

Durante a avaliação, o comando do personagem é avaliado e resulta em uma nota.
Um comando executado corretamente, ou seja, ataque em inimigo ou cura em aliado, resulta em nota positiva.
Um comando executado incorretamente, ou seja, ataque em aliado ou cura em inimigo, resulta em nota negativa.
Em todos os comandos este valor é somado ao fitness da equipe.

Os valores de fitness quando a batalha acaba são eventualmente usados na seleção,
tendo maior probabilidade de ser selecionado com um maior fitness.

E então através do crossover e mutação é criado uma população melhorada, com um desempenho melhor para batalhar e derrotar a equipe inimiga.

# Resultados
Após um treinamento de 50 gerações da Inteligência Artificial,
percebeu-se que os personagens aprenderam a melhorar sua performance com sucesso,
e com melhoras significativas nas primeiras gerações.

![Resultado](images/pt-BR/AI-Result.png)

# Vídeo
[![TCC - AI of RPG game: Youtube video](http://img.youtube.com/vi/blHZ4aY4BNU/0.jpg)](https://www.youtube.com/watch?v=blHZ4aY4BNU "TCC - AI of RPG game")

# AI RPG Game
The creation of an application of Artificial Intelligence in a turn-based RPG game.

[Click here](https://github.com/mateus-etto/Monografia-IAJogoRPG/releases) for the monograph (in Portuguese).
Please check de video below to see the implementation result.
