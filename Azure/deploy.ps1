$resource_group_name = 'skillsgarden'
$appserviceplan_name = 'skillsgardenplan'
$azure_function_name = 'skillsgardenfunction'
$storageaccount_name = 'skillsgardenstorage'

Write-Host "Deploying resources in $resource_group_name"

# Create a new resource-group
az group create -l westeurope -n $resource_group_name

# Deploy resources inside resource-group
az deployment group create --mode Incremental --resource-group $resource_group_name --template-file template-azure-function.json --parameters appService_name=$azure_function_name appServicePlan_name=$appserviceplan_name resourceGroup=$resource_group_name storageaccount_name=$storageaccount_name