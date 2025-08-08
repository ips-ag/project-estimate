<a id="readme-top"></a>

<!-- PROJECT SHIELDS -->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h3 align="center">Project Estimate</h3>

  <p align="center">
    AI-powered project estimation tool using intelligent agents to analyze requirements and provide accurate effort estimates
    <br />
    <a href="https://github.com/ips-ag/project-estimate"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/ips-ag/project-estimate">View Demo</a>
    &middot;
    <a href="https://github.com/ips-ag/project-estimate/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/ips-ag/project-estimate/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->

## About The Project

Project Estimate is an innovative AI-powered application that revolutionizes software project estimation by leveraging a multi-agent system. The application uses specialized AI agents working collaboratively to analyze project requirements, break them down into user stories and tasks, and provide accurate effort estimates for software development projects.

### Key Features

- **AI Agent Orchestration**: Multiple specialized AI agents (Consultant, Analyst, Architect, Developer) work together
- **Interactive Requirements Analysis**: AI agents ask clarifying questions to gather complete requirements
- **Automated Task Breakdown**: Converts requirements into structured user stories and development tasks
- **PERT-based Estimation**: Provides optimistic, realistic, and pessimistic estimates with calculated effort
- **Real-time Communication**: Uses SignalR for live interaction between AI agents and users
- **Document Processing**: Supports file uploads for requirement documents
- **Azure Integration**: Built for cloud deployment with Azure services

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With

- [![.NET][DotNet]][DotNet-url]
- [![Semantic Kernel][SemanticKernel]][SemanticKernel-url]
- [![SignalR][SignalR]][SignalR-url]
- [![React][React.js]][React-url]
- [![TypeScript][TypeScript]][TypeScript-url]
- [![Vite][Vite.js]][Vite-url]
- [![Azure][Azure]][Azure-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->

## Getting Started

This guide will help you set up and run the Project Estimate application locally.

### Prerequisites

Before running the application, ensure you have the following installed:

- **.NET 9.0 SDK**

  ```sh
  dotnet --version
  ```

- **Node.js (v20 or higher) and Yarn**

  ```sh
  node --version
  yarn --version
  ```

- **Azure subscription** with access to Azure OpenAI, Azure AI Document Intelligence, and Azure Storage account.

### Installation

1. **Clone the repository**

   ```sh
   git clone https://github.com/ips-ag/project-estimate.git
   cd project-estimate
   ```

2. **Configure API Keys**

   Create a `secrets.json` file in the `src/api` directory:

   ```json
   {
     "AzureOpenAI": {
       "Endpoint": "your-azure-openai-endpoint",
       "ApiKey": "your-api-key",
       "DeploymentName": "your-deployment-name"
     },
     "AzureDocumentIntelligence": {
       "Endpoint": "your-document-intelligence-endpoint",
       "ApiKey": "your-api-key"
     },
     "AzureStorageAccount": {
       "ConnectionString": "your-storage-connection-string"
     }
   }
   ```

3. **Install Backend Dependencies**

   ```sh
   cd src/api
   dotnet restore
   ```

4. **Install Frontend Dependencies**

   ```sh
   cd ../app
   yarn install
   ```

5. **Build and Run**

   **API (.NET):**

   ```sh
   cd src/api
   dotnet run
   ```

   **App (React):**

   ```sh
   cd src/app
   yarn dev
   ```

6. **Access the Application**

   Open your browser and navigate to `http://localhost:5173` (or the port specified by Vite)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->

## Usage

### Basic Workflow

1. **Provide Project Requirements**

   - Enter your project requirements as textual input in the chat interface
   - Alternatively, upload a requirements document (PDF, Word, etc.) for the system to process
   - The system supports various document formats through Azure AI Document Intelligence

2. **Analyst Verification Process**

   - The Analyst AI agent will review your requirements and begin verification
   - The analyst may ask clarifying questions about technical constraints, user count, use cases, integrations, security requirements, etc.
   - Answer the analyst's questions to ensure complete and accurate requirements gathering

3. **Review AI Agent Analysis**

   - Once requirements are verified, the Architect AI agent will break down requirements into user stories and tasks
   - The Developer AI agent will provide effort estimates using PERT methodology
   - Enable "Show Reasoning" to see the AI agents' thought processes during analysis

4. **Get Final Estimates**
   - Receive a comprehensive breakdown with optimistic, realistic, and pessimistic estimates
   - Results are presented in a structured Markdown table format with calculated effort values

### AI Agent Roles

- **Analyst AI Agent**: Verifies and clarifies project requirements through interactive questioning
- **Architect AI Agent**: Analyzes requirements and creates user stories. It further breaks them down into development tasks
- **Developer AI Agent**: Provides effort estimates in man-days using PERT estimation techniques

### File Upload Support

The application supports uploading requirement documents to provide additional context for the AI agents. Supported formats include all document types supported by Azure AI Document Intelligence, e.g. PDF, Word, Excel, JPEG, PNG, text files, Markdown, etc.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ROADMAP -->

## Roadmap

- [x] Multi-agent AI system implementation
- [x] Real-time communication with SignalR
- [x] Document processing capabilities
- [x] PERT-based estimation methodology
- [ ] Interaction with architect and developer AI agents
- [ ] Chat history
- [ ] Step back functionality

See the [open issues](https://github.com/ips-ag/project-estimate/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://github.com/ips-ag/project-estimate/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=ips-ag/project-estimate" alt="contrib.rocks image" />
</a>

<!-- LICENSE -->

## License

Distributed under the MIT License. See `LICENSE` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->

## Contact

IPS AG - [Website](https://www.ips-ag.com/) - [LinkedIn](https://www.linkedin.com/company/ips-ag/)

Project Link: [https://github.com/ips-ag/project-estimate](https://github.com/ips-ag/project-estimate)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->

[contributors-shield]: https://img.shields.io/github/contributors/ips-ag/project-estimate.svg?style=for-the-badge
[contributors-url]: https://github.com/ips-ag/project-estimate/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/ips-ag/project-estimate.svg?style=for-the-badge
[forks-url]: https://github.com/ips-ag/project-estimate/network/members
[stars-shield]: https://img.shields.io/github/stars/ips-ag/project-estimate.svg?style=for-the-badge
[stars-url]: https://github.com/ips-ag/project-estimate/stargazers
[issues-shield]: https://img.shields.io/github/issues/ips-ag/project-estimate.svg?style=for-the-badge
[issues-url]: https://github.com/ips-ag/project-estimate/issues
[license-shield]: https://img.shields.io/github/license/ips-ag/project-estimate.svg?style=for-the-badge
[license-url]: https://github.com/ips-ag/project-estimate/blob/master/LICENSE
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/company/ips-ag
[product-screenshot]: images/screenshot.png
[DotNet]: https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[DotNet-url]: https://dotnet.microsoft.com/
[React.js]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[React-url]: https://reactjs.org/
[TypeScript]: https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white
[TypeScript-url]: https://www.typescriptlang.org/
[Azure]: https://img.shields.io/badge/Microsoft_Azure-0089D0?style=for-the-badge&logo=microsoft-azure&logoColor=white
[Azure-url]: https://azure.microsoft.com/
[SemanticKernel]: https://img.shields.io/badge/Semantic_Kernel-512BD4?style=for-the-badge&logo=microsoft&logoColor=white
[SemanticKernel-url]: https://github.com/microsoft/semantic-kernel
[SignalR]: https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=microsoft&logoColor=white
[SignalR-url]: https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction
[Vite.js]: https://img.shields.io/badge/Vite-646CFF?style=for-the-badge&logo=vite&logoColor=white
[Vite-url]: https://vitejs.dev/
