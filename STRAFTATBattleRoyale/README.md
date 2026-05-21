# STRAFTAT Battle Royale Mod

Añade una zona que se va cerrando al estilo Battle Royale. Los jugadores fuera de la zona reciben daño hasta que solo quede uno.

---

## Requisitos

- STRAFTAT instalado en Steam
- Windows 10 / 11

---

## Instalación

### Paso 1 — Instalar BepInEx

1. Descarga el archivo `BepInEx_win_x64_5.4.x.x.zip` desde:
   **https://github.com/BepInEx/BepInEx/releases**
   (busca la versión más reciente que empiece por `5.4`)

2. Abre el ZIP y copia **todo su contenido** dentro de la carpeta del juego:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\STRAFTAT\
   ```
   > Si tienes Steam en otro disco, la ruta será diferente. Puedes encontrarla abriendo Steam → click derecho en STRAFTAT → Administrar → Ver archivos locales.

3. Lanza STRAFTAT una vez y ciérralo. Esto genera los archivos necesarios de BepInEx.

### Paso 2 — Instalar el mod

1. Descarga el archivo `STRAFTATBattleRoyale.dll`

2. Cópialo a esta carpeta (créala si no existe):
   ```
   STRAFTAT\BepInEx\plugins\STRAFTATBattleRoyale\
   ```

3. Listo. La próxima vez que abras el juego el mod estará activo.

---

## Cómo usarlo

Al empezar cualquier ronda la zona Battle Royale se activa automáticamente.



## Configuración

Después de la primera vez que arranques el juego con el mod instalado aparecerá este archivo:
```
STRAFTAT\BepInEx\config\com.tuusuario.straftat.battleroyale.cfg
```

Puedes abrirlo con el Bloc de notas y cambiar estos valores:

| Opción | Por defecto | Qué hace |
|---|---|---|
| RadioInicial | 120 | Tamaño inicial de la zona en metros |
| RadioFinal | 5 | Tamaño mínimo al que llega la zona |
| NumFases | 4 | Cuántas veces se cierra la zona |
| EsperaPorFase | 30 | Segundos entre cada cierre |
| DuracionReduccion | 60 | Segundos que tarda en cerrarse cada fase |
| DanoPorSegundo | 10 | Vida que pierdes por segundo fuera de la zona |

---

## Solución de problemas

**El mod no hace nada al entrar en partida**
Comprueba que BepInEx está bien instalado abriendo este archivo y verificando que hay líneas de texto:
```
STRAFTAT\BepInEx\LogOutput.log
```
Debe contener la línea: `[Info] STRAFTAT Battle Royale v1.0.0 cargado correctamente.`


