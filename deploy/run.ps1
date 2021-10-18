param ($resourceGroup='rgazfunc', $location='australiasoutheast', [Parameter(Mandatory)]$googleAccountEmail, [Parameter(Mandatory)]$googleSecret)

az group create --location $location --resource-group $resourceGroup
$deploymentName = $(Get-Date -Format "yyyy_MM_dd_hh_mm")
$userId = az ad signed-in-user show --query objectId --output tsv
az deployment group create --resource-group $resourceGroup --template-file main.bicep --parameters userId=$userId googleAccountEmail=$googleAccountEmail googleSecret=$googleSecret --name $deploymentName --query properties.outputs.functionName.value
$function = az deployment group show --name $deploymentName --resource-group $resourceGroup --output tsv --query properties.outputs.functionName.value --output tsv

cd ../src
func azure functionapp publish $function