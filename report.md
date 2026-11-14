# REPORT
## Prompt


Context: we are developing a web platform for collaborative, configurable thematic tourist itineraries (part of a Destination Management System), with a back office and a public interface for tourists. An editor defines a thematic itinerary made of ordered fixed stops (title, description, coordinates); a segment connects two consecutive fixed stops. Economic operators (restaurants, wineries, workshops, farms, accommodation) propose experiences, each tied to a single segment of a specific itinerary; the editor evaluates the proposals before publication. Tourists browse published itineraries, select the optional experiences they want, see an indicative total cost and get a personalised summary; they can remove fixed stops but not reorder them. 
Story points measure the relative effort of a story (amount of work, complexity, uncertainty), not hours. Use only the scale 1, 2, 3, 5, 8, 13.

Estimate each of the following user stories independently.

As an editor, I want to create a new thematic itinerary in draft status, so that I can progressively define its contents before publication.
As an editor, I want to add and edit the geolocated fixed stops of an itinerary, so that I can identify the places of interest that make up the route.
As an editor, I want to define and modify the order of an itinerary’s fixed stops, so that I can establish the sequence in which the places of interest are to be visited.
As an editor, I want to publish an itinerary while keeping the order of the defined fixed stops, so that tourists can browse the route I designed.
As a tourist, I want to browse the published thematic itineraries, so that I can find visit routes consistent with my interests.
As an editor, I want to open an itinerary to participation by economic operators, so that they can propose optional experiences to associate with its segments.
As an economic operator, I want to propose one of my experiences by associating it with a specific segment of an itinerary open to participation, so that I can offer my services to tourists following that itinerary.
As an economic operator, I want to specify whether a proposed experience is free or paid and indicate its price, if any, so that tourists can assess its cost before selecting it.
"As an economic operator, I want to find the itineraries open to participation that cross the territory in which I operate, so that I can identify opportunities
to propose my experiences."
As an economic operator, I want to receive a notification when an itinerary relevant to my area of activity is opened to participation, so that I can promptly identify new opportunities to promote my services.
As a tourist, I want to select and deselect the optional experiences associated with an itinerary’s segments, so that I can personalize the route according to my interests.
As a tourist, I want to see the total cost of the selected optional experiences, so that I can assess the expected expense for the personalized route.
As a tourist, I want to obtain a summary of my personalized itinerary, including the fixed stops and the selected optional experiences, so that I can review the route I intend to take.
As an editor, I want to review the experiences proposed by operators for an itinerary, so that I can verify their consistency with the theme and characteristics of the route.
As an editor, I want to approve or reject the experiences proposed by operators, so that only those deemed consistent with the itinerary are available to tourists.
As an economic operator, I want to modify or withdraw one of my proposed experiences, so that the published information is always consistent with the services I can actually offer.
As a tourist, I want to see the optional experiences available along the itinerary’s segments, so that I can learn about the visit opportunities and the activities offered by operators.
As a tourist, I want to see on a map the fixed stops and the selected experiences, so that I can understand the geographic distribution of the activities I intend to carry out.

Answer only, one line per story: "S<n>: <story points> - <reason in one sentence>"

## GEMINI

ANSWER: Gemini 4 Argon
S1: 2 - Requires standard CRUD functionality to initialize an itinerary entity with basic metadata and a default draft state.
S2: 5 - Involves form handling, coordinate validation or map-based geolocation picking, and automatic management of connecting segments.
S3: 3 - Requires an interactive reordering interface and backend logic to update sequence indices and recalculate consecutive segments.
S4: 2 - Implements a state transition from draft to published with validation checks to ensure fixed stops and their sequence are complete.
S5: 3 - Entails building a public catalog view with thematic filtering, pagination, and route previews for published itineraries.
S6: 1 - Consists of a simple status toggle on the itinerary entity to expose its segments to authenticated economic operators.
S7: 3 - Requires a submission workflow and data model associations linking an operator's experience to a specific itinerary segment in a pending state.
S8: 1 - Adds a conditional free/paid toggle and numeric price input with basic validation to the experience proposal form.
S9: 5 - Demands geospatial querying logic to match itinerary coordinates and segments against the operator's geographic territory.
S10: 5 - Combines event-driven triggers upon itinerary opening, background spatial matching, and an asynchronous notification delivery service.
S11: 2 - Involves interactive client-side state management allowing tourists to toggle optional experiences on specific segments within their session.
S12: 1 - Requires a straightforward reactive calculation summing the prices of all currently selected paid experiences.
S13: 3 - Aggregates the active fixed stops, selected segment experiences, and indicative total cost into a structured summary view.
S14: 2 - Provides a back-office read view grouping proposed experiences by segment so the editor can inspect details against the itinerary theme.
S15: 2 - Implements moderation state transitions that control whether proposed experiences become publicly visible to tourists.
S16: 3 - Covers edit and withdrawal operations along with workflow rules to re-trigger editor approval when an already approved experience is modified.
S17: 2 - Fetches and renders approved optional experiences nested under their respective route segments on the public itinerary view.
S18: 5 - Requires integrating an interactive map component to plot geolocated fixed stops and dynamically update markers as optional experiences are selected.

## GPT
S1: 3 - Creazione di un itinerario.   
S2: 5 - Aggiunta e modifica delle tappe geolocalizzate.   
S3: 5 - Definizione e modifica dell'ordine delle tappe.   
S4: 3 - Pubblicazione di un itinerario.   
S5: 5 - Consultazione degli itinerari pubblicati.   
S6: 2 - Apertura e chiusura della partecipazione degli operatori.   
S7: 5 - Proposta di un'esperienza da parte di un operatore.   
S8: 2 - Definizione del prezzo di un'esperienza.   
S9: 8 - Ricerca di itinerari tramite posizione geografica e raggio.   
S10: 8 - Notifiche per nuovi itinerari pertinenti.   
S11: 3 - Selezione e deselezione delle esperienze opzionali.   
S12: 2 - Calcolo del costo complessivo delle esperienze.   
S13: 5 - Riepilogo dell'itinerario personalizzato.   
S14: 3 - Consultazione delle proposte da parte dell'editor.   
S15: 5 - Approvazione o rifiuto delle esperienze.   
S16: 5 - Modifica o ritiro di un'esperienza proposta.   
S17: 3 - Visualizzazione delle esperienze opzionali disponibili.   
S18: 8 - Visualizzazione dell'itinerario personalizzato sulla mappa.  


## Qwen
MODEL: Qwen3.7-Plus

S1: 2 - Simple CRUD operation requiring minimal fields and basic database interaction.
S2: 3 - Requires map integration or coordinate validation adding moderate complexity to standard CRUD.
S3: 3 - Involves UI reordering logic and updating sequence indices in the backend.
S4: 2 - Simple state transition with validation to ensure required fields and order are intact.
S5: 3 - Requires a searchable, filterable list view with pagination and basic data fetching.
S6: 2 - Simple status update triggering visibility changes for economic operators.
S7: 3 - Requires a form linked to specific itinerary segments with validation and backend storage.
S8: 2 - Simple conditional field logic and basic data validation on the existing proposal form.
S9: 5 - Requires geospatial querying or territory matching logic, adding significant backend complexity.
S10: 5 - Involves setting up a notification system and matching operator territories to newly opened itineraries.
S11: 3 - Requires interactive UI state management and real-time updates to the user's selection.
S12: 2 - Simple real-time aggregation of selected experience prices on the frontend.
S13: 3 - Requires aggregating fixed stops and selected experiences into a coherent, readable summary view.
S14: 3 - Needs a dedicated back-office view to list and inspect details of pending proposals.
S15: 2 - Simple state transition with basic audit logging for the editor's decision.
S16: 3 - Requires conditional logic to prevent changes after approval and standard update or delete operations.
S17: 3 - Involves fetching and displaying grouped data in a user-friendly format for tourists.
S18: 5 - Requires integrating a mapping library to plot multiple distinct markers with custom popups based on user selections.


## NOTE 
We also noticed a decrease in ratings after specifying to consider dependencies between stories. The reported values are the ones before such decrease. 

Our prediction deviated from the one generated by LLMs, especially regarding S3, S6, S7, S10, S13, S15, S16 and S18.

On S18 the LLMs might have overestimated the difficulty due to the non-UI requirement.