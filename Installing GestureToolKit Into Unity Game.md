# Installing GestureToolKit Into Unity Game in Console

1. *Change to your Unity project directory*

	**macOS/Linux (bash/zsh/fish):** `cd <path-to-Unity-project>`

	**Windows (PowerShell/cmd):**    `cd <path-to-Unity-project>`

2. *Print the working directory to make sure you’re in the correct folder*

	**macOS/Linux (bash/zsh/fish):** `pwd`

	**Windows (PowerShell):**        `pwd        /*alias of Get-Location*/`

	**Windows (cmd):**               `cd         <no args>   /*or*/   echo %CD%`

3. *Remove any existing virtual environment named .venv (if necessary)*

	**macOS/Linux (bash/zsh):**      `rm -rf .venv`

	**Windows (PowerShell):**        `Remove-Item -Recurse -Force .venv`

	**Windows (cmd):**               `rmdir /s /q .venv`

4. *Create a new Python virtual environment in .venv*

	**macOS/Linux (bash/zsh/fish):** `python3 -m venv .venv`

	**Windows (PowerShell/cmd):**    `py -3 -m venv .venv   /*or*/   python -m venv .venv`

5. *Activate the virtual environment*

	**macOS/Linux (bash/zsh):**      `source .venv/bin/activate`

	`macOS/Linux (fish):          source .venv/bin/activate.fish`

	**Windows (PowerShell):**        `.\.venv\Scripts\Activate.ps1`

	**Windows (cmd):**               `.\.venv\Scripts\activate.bat`

6. *Install the slrtk package into the active environment*

	**All OS:**                      `pip install slrtk`

	**(also if you want):**          `python -m pip install --upgrade pip && python -m pip install slrtk`

7. *Open the current folder in the system file browser (to make sure you are where you think you are)*

	**macOS:**                       `open .`

	**Linux:**                       `xdg-open .`

	**Windows (PowerShell/cmd):**    `start .`

8. *Install the toolkit’s GTK into the Unity project in the current folder*

	**All OS:**                      `slrtk gtk install --unity --path .`
