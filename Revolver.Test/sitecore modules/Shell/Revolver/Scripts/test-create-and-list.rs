@stoponerror

test-init create-and-list

# create several items
set salt < (random 10000)
touch -t (sample/sample item) alpha$salt$
touch -t (sample/sample item) beta$salt$
touch -t (sample/sample item) gamma$salt$

# list all items
ls > if (($~$) !? alpha$salt$) (exit (missing item 'alpha'))
ls > if (($~$) !? beta$salt$) (exit (missing item 'beta'))
ls > if (($~$) !? gamma$salt$) (exit (missing item 'gamma'))

# list by name
ls -r al > if (($~$) !? alpha$salt$) (exit (missing item 'alpha' with regex))

# list using path
cd ..
ls -r al $testfolder$ > if (($~$) !? alpha$salt$) (exit (missing item 'alpha' using path))

# cleanup
rm $testfolder$

echo ** Test Complete **