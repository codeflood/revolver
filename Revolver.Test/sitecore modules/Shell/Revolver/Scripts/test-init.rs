if ($1$ = ()) (exit (Missing test folder name))

cd /
set testfolder test-$1$

# remove existing test folder if it exists
find -a name $testfolder$ -so > if ($~$ != 0) (rm $testfolder$)

# create test folder
create -t (common/folder) $testfolder$
cd $testfolder$
