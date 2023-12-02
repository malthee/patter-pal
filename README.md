# PatterPal - Personal Language Teacher
*This project is a submission for the [Microsoft AI Classroom Hackathon](https://microsoftaiclassroom.devpost.com/).*

![PatterPal Screenshot](docs/img/uimain.PNG)

Immerse yourself in language learning: Talk freely on your chosen topics, get live feedback, and engage with a smart conversation partner in multiple languages. Learn your way with PatterPal!

## How it works
PatterPal is a web application that allows you to practice your speaking skills in a foreign language. *It's like having a language teacher just for yourself.*  PatterPal will listen to you and give you feedback on your pronunciation and fluency, while also talking with you about your chosen topic.

## Technologies
- Azure Speech Services
  - Speech to Text
  - Pronounciation Assessment
  - Synthesis
- Azure App Services
  - ASP.NET Core Web App
- Azure Cosmos DB
- Google OAuth 2.0
  
<p>&nbsp;</p>
<div align="center">
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/121405384-444d7300-c95d-11eb-959f-913020d3bf90.png" alt="C#" title="C#"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/183911544-95ad6ba7-09bf-4040-ac44-0adafedb9616.png" alt="Microsoft Azure" title="Microsoft Azure"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/187070862-03888f18-2e63-4332-95fb-3ba4f2708e59.png" alt="websocket" title="websocket"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/192107858-fe19f043-c502-4009-8c47-476fc89718ad.png" alt="REST" title="REST"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/117447155-6a868a00-af3d-11eb-9cfe-245df15c9f3f.png" alt="JavaScript" title="JavaScript"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/192158954-f88b5814-d510-4564-b285-dff7d6400dad.png" alt="HTML" title="HTML"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/183898674-75a4a1b1-f960-4ea9-abcb-637170a00a75.png" alt="CSS" title="CSS"/></code>
	<code><img width="64" src="https://user-images.githubusercontent.com/25181517/192108374-8da61ba1-99ec-41d7-80b8-fb2f7c0a4948.png" alt="GitHub" title="GitHub"/></code>
    <code><img alt="PatterPal" title="PatterPal" width="64" height="64" src="docs/img/logo.svg"/></code>
</div>

## Project Team
We are based in Austria and currently studying Software Engineering at the [University of Applied Sciences Upper Austria](https://www.fh-ooe.at/en/hagenberg-campus/).

**Members:**
- [Marcel Salvenmoser](https://github.com/malthee)
- [Stefan Weißensteiner](https://github.com/seventinnine)

## Testing Instructions

Visit [https://patter-pal.azurewebsites.net](https://patter-pal.azurewebsites.net)

Login:
- either use the Special Access Code we have provided or
- or connect your Gmail-Account with the application
  - PatterPal only requests the scope necessary for reading the email address from the token. PatterPal does not request any private user data

<img height="500" src="https://github.com/malthee/patter-pal/assets/58472456/72e03155-2a4a-4627-830f-2afcb681846a" alt="PatterPal Login"/>

Select a language of your choice and click the round button that resembles a microphone.

<img height="500" src="https://github.com/malthee/patter-pal/assets/58472456/ea3a090a-c29c-4bb5-a946-46a83ffce1d2" alt="PatterPal Language Select"/>

If this is your first visit (or depending on your browser settings), you will need to allow the website to use your microphone.
![Microphone Permissions](https://github.com/malthee/patter-pal/assets/58472456/5118ca73-dc6a-4de2-a55d-db6a7a2f96c5)

Click the 🎙️ button and start talking. A few seconds after speaking, your spoken text will gradually show up.
The recording will stop after some moments of silence or if you manually click the 🎙️ button again.

<img height="500" src="https://github.com/malthee/patter-pal/assets/58472456/6f671f3d-bf90-429a-9bbb-376b34b7070d" alt="PatterPal Start Recording"/>

A few seconds after the recording has halted, your language teacher will gradually respond.
After the response has finished generating, the response will be read to you via Speech-to-Text.
Below the language selelection box, you can the metrics regarding your spoken words. Misprounciations will also be highlighed in your spoken text.
If you want to stop the Speech-to-Text output, you can click the ✋ button.
Also keep in mind that you can change the langauge of the conversation whenever you want.

<img height="500" src="https://github.com/malthee/patter-pal/assets/58472456/97092df2-adba-4e39-b335-5472bce52c80" alt="PatterPal Responding"/>

If you want to your conversation history, you can press the 📃 button on the top right of the screen.
It toggles the your conversation history and allows you to start a new conversation
You can also rename or delete individual conversations here.

<img height="500" src="https://github.com/malthee/patter-pal/assets/58472456/1eb30964-cd7c-43e4-8fc5-cc2fd9060c83" alt="PatterPal Conversation History"/>

After you had a few conversations with your language teacher, you can visit the stats page by clicking the 📊 button on the top right.
Here you can see how your accuracy has changed over time or what words were least accurately pronounced.
You can filter your metrics by *language* and also adjust the analysed time period and time resolution (playing around with these values is a good idea if you've been PatterPal for an extended period of time).

<img height="500" src="https://github.com/malthee/patter-pal/assets/58472456/d366a003-8851-492f-bbc7-8ba0d801066c" alt="PatterPal Stats"/>

You can get back to the application by clicking the PatterPal icon on the top left.
When you are done you can use the 🚪 button on the top right to log out.

## Diagrams
### Data Layer Diagram
![Data Layer Diagram](docs/img/datalayer.svg)

### WebSocket Communication Workflow
![WebSocket Communication](docs/img/communication.svg)

## Privacy Policy

### Information Collection
- Email-Based Accounts: If you register for an account using your email, we collect and store your email address. This is used for account verification.
- Conversations and Chats: We record and store the details of your conversations and chats with PatterPal. This includes both your input and the responses from PatterPal. This is required so you can access past conversations anytime.
- Pronunciation Analysis: When speaking with PatterPal, we collect and analyze data on your pronunciation accuracy and the mistakes made during spoken text exercises. This information is used to provide personalized feedback and improve your learning experience.

### How We Use Your Information
The information we collect is used to:
- Provide, operate, and maintain our services.
- Improve, personalize, and expand our services.
- Understand and analyze how you use our services.
- Develop new products, services, features, and functionality.
- Communicate with you for service-related purposes.

You can delete your *Conversation, Chat and Pronunciation Analysis* data on our [Privacy Page](https://patter-pal.azurewebsites.net/Home/Privacy).
