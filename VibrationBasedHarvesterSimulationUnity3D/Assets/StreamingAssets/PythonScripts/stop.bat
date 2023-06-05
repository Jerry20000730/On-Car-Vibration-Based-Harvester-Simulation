@echo off
set port=5555
set pid=0

for /f "tokens=2,5" %%b in ('netstat -ano ^| findstr ":%port%"') do (
    set temp=%%b
    for /f "usebackq delims=: tokens=1,2" %%i in (`set temp`) do (
      if %%j==%port% (
        taskkill /f /pid %%c
        set pid=%%c
      ) else (
        echo process not found
      )
    )
    if !pid!==0 (
        echo process not found
    )
)

exit