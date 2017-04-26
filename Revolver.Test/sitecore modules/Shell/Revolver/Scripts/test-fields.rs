@stoponerror

test-init fields

# create item and set some fields
touch -t (sample/sample item) item
cd item
sf title (the title)
gf -f title > if (($~$) != (the title)) (exit (title field is wrong))
gf > if (($~$) !? __Created) (exit (all field listing is wrong))

# using path
cd ..
sf text (<p>lorem</p>) item
gf -f text item > if (($~$) != (<p>lorem</p>)) (exit (text field is wrong))

# no version
touch -t (sample/sample item) noversion
cd noversion
sf -nv title hello
lsv > if (($~$) !? (1 version)) (exit (Version count is wrong))

# reset standard value
sf __renderings (bad value)
sf -r __renderings
gf -f __renderings > if (($~$) !? <r) (exit (field didn't reset to standard values))

# cleanup
cd /
rm $testfolder$

echo ** Test Complete **