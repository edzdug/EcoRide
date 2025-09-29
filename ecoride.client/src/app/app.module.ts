import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AccueilComponent } from './accueil/accueil.component';
import { ItineraireformComponent } from './itineraireform/itineraireform.component';
import { ItinerairevueComponent } from './itinerairevue/itinerairevue.component';
import { InscriptionComponent } from './inscription/inscription.component';
import { AuthentificationComponent } from './authentification/authentification.component';
import { TokenInterceptor } from './authentification/token.interceptor';
import { ProfilComponent } from './profil/profil.component';
import { CovoiturageDetailComponent } from './covoiturage-detail/covoiturage-detail.component';
import { SaisieCovoiturageComponent } from './saisie-covoiturage/saisie-covoiturage.component';
import { ReactiveFormsModule } from '@angular/forms';
import { HistoriqueComponent } from './historique/historique.component';
import { SaisieAvisComponent } from './saisie-avis/saisie-avis.component';

@NgModule({
  declarations: [
    AppComponent,
    AccueilComponent,
    ItineraireformComponent,
    ItinerairevueComponent,
    InscriptionComponent,
    AuthentificationComponent,
    ProfilComponent,
    CovoiturageDetailComponent,
    SaisieCovoiturageComponent,
    HistoriqueComponent,
    SaisieAvisComponent,
  ],
  imports: [
    BrowserModule, HttpClientModule, ReactiveFormsModule,
    AppRoutingModule, FormsModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
