// Extracted from the btros AI Search RAG slice (ark360-hq/btros#473).
// This module declares the Azure AI Search service the governed gateway
// uses for POST /v1/retrieve. It does not deploy anything from this
// repository and it does not create Entra or Stripe resources.
//
// Do not query this service from a raw Azure AI Search SDK. Product AI
// retrieval goes through the Intelligence gateway.

@description('Azure region. Adopter supplies this. This file does not deploy.')
param location string

@description('Azure AI Search service name.')
param searchServiceName string

@description('SKU name valid in the adopter tenant. No resource is created here.')
param skuName string

@description('Replica count. Adopter sets this in their tenant.')
@minValue(1)
param replicaCount int = 1

@description('Partition count. Adopter sets this in their tenant.')
@minValue(1)
param partitionCount int = 1

@description('Index name the gateway retrieve operation reads.')
param indexName string

resource searchService 'Microsoft.Search/searchServices@2023-11-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: skuName
  }
  properties: {
    replicaCount: replicaCount
    partitionCount: partitionCount
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    disableLocalAuth: true
  }
}

@description('Search endpoint only. Keys stay in the adopter secret store.')
output endpoint string = 'https://${searchService.name}.search.windows.net'
output indexName string = indexName
