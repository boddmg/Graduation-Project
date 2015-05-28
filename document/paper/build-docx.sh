pandoc --filter pandoc-citeproc --csl="ieee.csl" --biblio all.bib -f markdown+tex_math_dollars  $1 -o $1.tex
pandoc -o $1.docx $1.tex