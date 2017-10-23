@stoponerror

test-init search

# create several items
set salt < (random 10000)
create -t (sample/sample item) alpha$salt$
create -t (sample/sample item) beta$salt$ > set betaid $~$
create -t (sample/sample item) gamma$salt$ > set gammaid $~$
sf title (optimus$salt$) gamma$salt$

ls
echo beta$salt$

# search for items
search -w 20 -ns _name:beta$salt$ (ga -a id) > if (($~$) != $betaid$) (exit (failed to find item 'beta'))
search -ns title:optimus$salt$ (ga -a id) > if (($~$) != $gammaid$) (exit (failed to find item 'gamma'))

# cleanup
test-teardown

echo ** Test Complete **