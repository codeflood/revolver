@stoponerror

test-init alias

alias dir ls
dir

cd /sitecore/content
alias getname ga -a name
getname > if ($~$ != content) (exit ('getname' alias not working))

# cleanup
cd /
rm $testfolder$
alias dir
alias getname

@continueonerror
dir > if ($~$ !? (unknown command)) (exit ('dir' alias not removed))
getname > if ($~$ !? (unknown command)) (exit ('getname' alias not removed))

echo ** Test Complete **