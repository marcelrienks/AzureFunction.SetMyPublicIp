# AzureFunction.SetMyPublicIp
AzureFunction.SetMyPublicIp

[![Build & Test](https://github.com/marcelrienks/AzureFunction.SetMyPublicIp/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/marcelrienks/AzureFunction.SetMyPublicIp/actions/workflows/dotnetcore.yml)

# SetMyPublicIp
This is a DDNS (Dynamic DNS) solution implemented using Azure.

The goal is to have some sort of recurring script, which will call an API function through, executing this Azure function, which updates the record set of a domain registered in Azure to point to the public IP address of the network that executed the recurring script.

**Note:** The same solution could be achieved by simply making an SDK call from some sort of recurring script, that will update the record set of a domain registered in Azure, removing the need for an Azure Function.  
However the purpose of this project was to learn how to use Azure Function to solve the problem.

## WIP (Work In Progress)
This is still a work in progress
### Todo:
* Complete Unit tests
* Test the project
