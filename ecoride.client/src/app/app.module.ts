import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AccueilComponent } from './accueil/accueil.component';
import { ItineraireformComponent } from './itineraireform/itineraireform.component';
import { ItinerairevueComponent } from './itinerairevue/itinerairevue.component';

@NgModule({
  declarations: [
    AppComponent,
    AccueilComponent,
    ItineraireformComponent,
    ItinerairevueComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
