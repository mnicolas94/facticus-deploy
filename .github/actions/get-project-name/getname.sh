while IFS=: read -r key value
do
    if [[ $key == "  productName" ]]
    then
		echo $value;
	fi
done < "ProjectSettings/ProjectSettings.asset"