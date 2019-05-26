# change 'pandoc.exe' to 'pandoc' if you are on a unix system and not on my weird WSL setup

pandoc.exe *.md --toc --include-in-header clearpage-between-sections.tex -N --template eisvogel.tex -s --from markdown --listings --variable urlcolor=cyan -o documentation.pdf

echo "Finished building documentation. Copying to package for release..."

cp documentation.pdf ../src/CoreLibrary/Assets/Scripts/CoreLibrary/documentation.pdf
