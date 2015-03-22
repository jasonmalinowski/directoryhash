# directoryhash

A simple tool that takes a directory and computes SHA-1 and SHA-256 hashes of all the files inside it. It can be used to verify transfers of files across a network, check for disk corruption, and other common uses.

## Usage

    directoryhash recompute

Computes the hashes of all files in the current working directory, and saves it in an XML file Hashes.xml.

    directoryhash update

Looks for files that were modified since the Hashes.xml was computed/updated, and incrementally updates it. Files are counted as modified if they have a creation or modification date after a timestamp that's stored in the Hashes.xml file.

    directoryhash purge [--dry-run] directory [directory...]

*Permanently* deletes any file from the current working directory that is a duplicate of a file in one of the other given directories. Files are matched by content only; file names and directory names are ignored. The Hashes.xml in all directories involved are used as the source of hashes; any files modified since the Hashes.xml are skipped. `--dry-run` can be passed which skips actually deleting files.