# Yu-Gi-Oh! Shadow Duelers — Arcade Edition

Juego de batalla por turnos desarrollado en **C# Windows Forms (.NET 8)** inspirado en Yu-Gi-Oh!

---

## 📋 Requisitos previos

Antes de instalar el proyecto necesitas tener instalado:

- **Windows 10/11**
- **Visual Studio 2022** (cualquier edición, incluyendo Community que es gratuita)
  - Descarga: https://visualstudio.microsoft.com/es/downloads/
  - Durante la instalación selecciona el workload: **"Desarrollo de escritorio de .NET"**
- **Git** (opcional, para clonar el repositorio)
  - Descarga: https://git-scm.com/downloads

---

## 🚀 Instalación paso a paso

### Opción A — Clonar con Git (recomendado)

1. Abre **Git Bash** o la terminal de tu preferencia
2. Navega a la carpeta donde quieres guardar el proyecto
3. Ejecuta:
```bash
git clone https://github.com/spencerrrrd-boop/YuGiOhShadowDuelers.git
```
4. Entra a la carpeta:
```bash
cd YuGiOhShadowDuelers
```

### Opción B — Descargar ZIP

1. Ve a https://github.com/spencerrrrd-boop/YuGiOhShadowDuelers
2. Click en el botón verde **"Code"**
3. Click en **"Download ZIP"**
4. Extrae el ZIP en la carpeta que prefieras

---

## ▶️ Abrir y ejecutar en Visual Studio

1. Abre **Visual Studio 2022**
2. Click en **"Abrir un proyecto o solución"**
3. Navega hasta la carpeta del proyecto y selecciona el archivo **`YuGiOhShadowDuelers.sln`**
4. Espera a que Visual Studio cargue el proyecto (puede tardar unos segundos la primera vez)
5. Presiona **`Ctrl + F5`** para compilar y ejecutar
   - O click en el botón verde **▶ Iniciar** en la barra superior

> ⚠️ Si Visual Studio muestra un error de SDK, ve a **Herramientas → Obtener herramientas y características** y asegúrate de tener instalado **.NET 8** dentro del workload de escritorio.

---

## 🎮 Cómo jugar

1. En la pantalla de título presiona **ENTER** o click en **INICIAR JUEGO**
2. Elige tu carta con las flechas **← →** del teclado (o click directo sobre la carta)
3. Confirma con **ENTER** o click en **CONFIRMAR SELECCIÓN**
4. Presiona **INICIAR BATALLA** y observa el duelo
5. Al terminar puedes **JUGAR DE NUEVO** o **SALIR**

---

## 🃏 Cartas disponibles

| Carta | HP | Daño | Esquiva |
|-------|----|------|---------|
| Slifer el Dragón del Cielo | 1000 | 10 – 130 | 15% |
| El Dragón Alado de Ra | 1000 | 15 – 125 | 12% |
| Mago Oscuro | 1000 | 10 – 135 | 10% |
| Maga Oscura | 1000 | 20 – 122 | 18% |
| **Seto Kaiba (máquina)** | 1000 | 10 – 120 | 20% |

---

## ⚔️ Reglas de batalla

- Vida inicial: **1000 HP** para ambos jugadores
- Cada turno el daño es **aleatorio** dentro del rango de cada carta
- Si el daño es el **máximo posible** → el defensor **pierde su siguiente turno** y regenera **10%** del daño recibido
- Cada turno se regenera **5%** del daño recibido automáticamente
- Cada personaje tiene su propia probabilidad de **esquivar** el ataque
- Pierde quien llegue primero a **0 HP**

---

## 📁 Estructura del proyecto

```
YuGiOhShadowDuelers/
├── Program.cs                    # Punto de entrada
├── CardStats.cs                  # Estadísticas de cada carta
├── BattleEngine.cs               # Lógica de batalla completa
├── SpriteManager.cs              # Carga y caché de imágenes
├── FormTitle.cs                  # Pantalla de título
├── FormCardSelect.cs             # Pantalla de selección de carta
├── FormBattle.cs                 # Pantalla de batalla
├── YuGiOhShadowDuelers.csproj   # Archivo de proyecto (.NET 8)
├── YuGiOhShadowDuelers.sln      # Solución de Visual Studio
└── Resources/
    ├── sprites/                  # Sprites de personajes (160×200px PNG)
    └── backgrounds/              # Fondos de pantalla
```

---

## 🛠️ Tecnologías utilizadas

- **C#** — Lenguaje de programación
- **.NET 8** — Framework
- **Windows Forms** — Interfaz gráfica
- **GDI+** — Renderizado de sprites y animaciones
