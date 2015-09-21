# Revolver #

Revolver is a command line and scripting tool for Sitecore. It saves developers and administors time by allowing them to script common and repetative actions.

Revolver also supports custom tooling by allowing developers to create their own commands to be used and combined with the OOTB commands.

## Examples ##

These are some quick examples of Revolver scripting:

	# navigate the content tree
	cd child-item
	cd ../sibling
	
	# list the children of the current item
	ls
	
	# get and set fields
	gf -f title
	sf text (this is the new text)
	
	# get and set attributes of the item
	ga
	ga -a id
	sa template {76036F5E-CBCE-46D1-AF0A-4143F9B557AA}
	
	# create a new item
	touch -t (sample/sample item) newitem
	
	# locate all items based on a template
	cd /
	find -r -t {76036F5E-CBCE-46D1-AF0A-4143F9B557AA} pwd
	
	# do the same using a custom content search index and only print names
	csearch -i myindex _template:{76036F5E-CBCE-46D1-AF0A-4143F9B557AA} (ga -a name)

## References ##

Additional information can be found on the [codeflood](http://www.codeflood.net) website under the [revolver](http://www.codeflood.net/revolver) section.