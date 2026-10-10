// Extracted from the btros Azure OpenAI adapter (ark360-hq/btros#472).
// This module declares the Azure OpenAI account and the chat plus embeddings
// deployments the governed gateway calls. It does not deploy anything from
// this repository and it does not create Entra or Stripe resources.
//
// Do not call this account from a raw Azure OpenAI SDK. Product AI goes
// through the Intelligence gateway (.arkos/contracts/intelligence-gateway.md).

@description('Azure region. Adopter supplies this. This file does not deploy.')
param location string

@description('Azure OpenAI account name.')
param accountName string

@description('SKU name valid in the adopter tenant. No resource is created here.')
param skuName string

@description('Custom subdomain used to build the account endpoint.')
param customSubDomainName string

@description('Chat deployment name used by POST /v1/complete.')
param chatDeploymentName string

@description('Chat model name, supplied by the adopter.')
param chatModelName string

@description('Chat model version, supplied by the adopter.')
param chatModelVersion string

@description('Chat deployment capacity in thousands of tokens per minute.')
@minValue(1)
param chatCapacity int = 1

@description('Embeddings deployment name used by the RAG path from #473.')
param embeddingsDeploymentName string

@description('Embeddings model name, supplied by the adopter.')
param embeddingsModelName string

@description('Embeddings model version, supplied by the adopter.')
param embeddingsModelVersion string

@description('Embeddings deployment capacity in thousands of tokens per minute.')
@minValue(1)
param embeddingsCapacity int = 1

resource account 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: accountName
  location: location
  kind: 'OpenAI'
  sku: {
    name: skuName
  }
  properties: {
    customSubDomainName: customSubDomainName
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }
}

resource chatDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: chatDeploymentName
  sku: {
    name: 'Standard'
    capacity: chatCapacity
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: chatModelName
      version: chatModelVersion
    }
  }
}

resource embeddingsDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: embeddingsDeploymentName
  sku: {
    name: 'Standard'
    capacity: embeddingsCapacity
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: embeddingsModelName
      version: embeddingsModelVersion
    }
  }
}

@description('Account endpoint only. Keys stay in the adopter secret store.')
output endpoint string = account.properties.endpoint
output chatDeploymentName string = chatDeployment.name
output embeddingsDeploymentName string = embeddingsDeployment.name
