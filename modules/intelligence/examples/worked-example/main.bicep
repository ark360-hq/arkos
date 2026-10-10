// Worked example: compose the extracted OpenAI and Search modules.
// Follow modules/intelligence/examples/worked-example/README.md.
// Do not deploy this file from the ARK OS repository.
// Product AI calls go through the governed gateway, not a raw SDK.

targetScope = 'resourceGroup'

@description('Azure region for both modules.')
param location string = resourceGroup().location

@description('Azure OpenAI account name.')
param openAiAccountName string

@description('Azure OpenAI SKU name supplied by the adopter.')
param openAiSkuName string

@description('Custom subdomain for the OpenAI endpoint.')
param openAiCustomSubDomainName string

@description('Chat deployment name.')
param chatDeploymentName string

@description('Chat model name.')
param chatModelName string

@description('Chat model version.')
param chatModelVersion string

@description('Embeddings deployment name.')
param embeddingsDeploymentName string

@description('Embeddings model name.')
param embeddingsModelName string

@description('Embeddings model version.')
param embeddingsModelVersion string

@description('Azure AI Search service name.')
param searchServiceName string

@description('Azure AI Search SKU name supplied by the adopter.')
param searchSkuName string

@description('Index name used by POST /v1/retrieve.')
param searchIndexName string

module openAi '../../bicep/openai.bicep' = {
  name: 'intelligence-openai'
  params: {
    location: location
    accountName: openAiAccountName
    skuName: openAiSkuName
    customSubDomainName: openAiCustomSubDomainName
    chatDeploymentName: chatDeploymentName
    chatModelName: chatModelName
    chatModelVersion: chatModelVersion
    embeddingsDeploymentName: embeddingsDeploymentName
    embeddingsModelName: embeddingsModelName
    embeddingsModelVersion: embeddingsModelVersion
  }
}

module search '../../bicep/search.bicep' = {
  name: 'intelligence-search'
  params: {
    location: location
    searchServiceName: searchServiceName
    skuName: searchSkuName
    indexName: searchIndexName
  }
}

output openAiEndpoint string = openAi.outputs.endpoint
output searchEndpoint string = search.outputs.endpoint
output searchIndexName string = search.outputs.indexName
