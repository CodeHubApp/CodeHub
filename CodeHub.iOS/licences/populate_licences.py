#!/usr/bin/python

import os
import sys
import plistlib
from copy import deepcopy

os.chdir(sys.path[0])

plist = {'PreferenceSpecifiers': [], 'StringsTable': 'Acknowledgements'}
base_group = {'Type': 'PSGroupSpecifier', 'FooterText': '', 'Title': ''}

for filename in os.listdir("."):
    if filename.endswith(".license"):
        current_file = open(filename, 'r')
        group = deepcopy(base_group)
        title = filename.split(".license")[0]
        group['Title'] = title
        group['FooterText'] = current_file.read()
        plist['PreferenceSpecifiers'].append(group)

plistlib.writePlist(
    plist,
    "../Settings.bundle/Acknowledgements.plist"
)

