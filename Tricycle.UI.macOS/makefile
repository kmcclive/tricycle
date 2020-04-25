makefile:

notarize:
	xcrun altool --notarize-app --primary-bundle-id "com.astound.tricycle" --username $(username) --password "$(password)" --file bin/Release/Tricycle.dmg

staple:
	xcrun stapler staple bin/Release/Tricycle.dmg
