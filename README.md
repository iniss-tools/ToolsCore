# ToolsCore

Pomocná knižnica pre INISSTools projekty.

## Expressions – jazyk výrazov INISS

`ToolsCore.Expressions` je prekladač a vyhodnocovač jazyka, ktorým INISS píše
podmienky v `TabTab.txt` a dynamické hodnoty v `StateDgm.txt`. Správanie kopíruje
INISS 3.39 vrátane jeho zvláštností (spojky na jednej úrovni s pravou
asociativitou, `PRIZNAK(x)` vracia masku, čísla cez `strtol(…, 0)`) – špecifikácia
je v dokumentácii `iniss-tools-docs/docs/iniss/formaty-suborov/local/vyrazy.mdx`.

| Trieda           | Účel                                                                                        |
|------------------|---------------------------------------------------------------------------------------------|
| `ExprLexer`      | tokeny s pozíciami; identifikátory rozpoznáva ako funkcie / konštanty / `Typ_…`             |
| `ExprParser`     | `Parse(text, context, symbols)` → strom `ExprNode` alebo prvá chyba s textom INISSu         |
| `ExprValidator`  | `Validate(text, options)` → preklad + varovania nad rámec INISSu (priorita spojok, neznáma stanica, maska porovnaná s číslom …) |
| `ExprEvaluator`  | vyhodnotenie stromu pre vlak (`IExprTrainContext`) s kontextom tabule (`ExprEvalSite`)       |
| `ExprFunctions`, `ExprConstants`, `ExprTrainTypes` | register funkcií, konštánt a zabudovaná tabuľka druhov vlakov (indexy 0–94) |

`IExprSymbolProvider` dodáva dáta grafikonu (kľúče `TrTypes.txt`, stanice, koľaje,
dopravcovia); bez neho sa `Typ_…` hľadá len medzi zabudovanými druhmi a kontroly
proti dátam sa preskočia.

Testy (`ToolsCore.Tests/Expressions`) overujú gramatiku, sémantiku a korpus výrazov
z reálnych súborov (`TestData/expressions.txt`); test `LiveData_AllConditionsCompile`
prejde všetky `TabTab.txt`/`StateDgm.txt` pod `D:\INISSroot`, ak priečinok existuje.
