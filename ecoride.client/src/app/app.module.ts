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

@NgModule({
  declarations: [
    AppComponent,
    AccueilComponent,
    ItineraireformComponent,
    ItinerairevueComponent,
    InscriptionComponent,
    AuthentificationComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
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
