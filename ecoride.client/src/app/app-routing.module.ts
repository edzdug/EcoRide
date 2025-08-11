import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { AccueilComponent } from './accueil/accueil.component';
import { ItineraireformComponent } from './itineraireform/itineraireform.component';

const routes: Routes = [
  { path: 'home', component: AppComponent },
  { path: 'accueil', component: AccueilComponent },
  { path: 'itineraireform', component: ItineraireformComponent },
  { path: '', redirectTo: '/accueil', pathMatch: 'full' } // Redirige vers la page1 par défaut];
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
