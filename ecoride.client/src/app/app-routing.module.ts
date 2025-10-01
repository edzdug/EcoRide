import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { AccueilComponent } from './accueil/accueil.component';
import { ItineraireformComponent } from './itineraireform/itineraireform.component';
import { ItinerairevueComponent } from './itinerairevue/itinerairevue.component';
import { InscriptionComponent } from './inscription/inscription.component';
import { AuthentificationComponent } from './authentification/authentification.component';
import { ProfilComponent} from './profil/profil.component';
import { AuthGuard } from './authentification/auth.guard';
import { CovoiturageDetailComponent } from './covoiturage-detail/covoiturage-detail.component';
import { SaisieCovoiturageComponent } from './saisie-covoiturage/saisie-covoiturage.component';
import { HistoriqueComponent } from './historique/historique.component';
import { SaisieAvisComponent } from './saisie-avis/saisie-avis.component';
import { EspaceEmployeComponent } from './espace-employe/espace-employe.component'
import { EmployeGuard } from './authentification/employe.guard';

const routes: Routes = [
  { path: 'home', component: AppComponent },
  { path: 'accueil', component: AccueilComponent },
  { path: 'itineraireform', component: ItineraireformComponent },
  { path: 'itinerairevue', component: ItinerairevueComponent },
  { path: 'inscription', component: InscriptionComponent },
  { path: 'login', component: AuthentificationComponent },
  { path: 'profil', component: ProfilComponent, canActivate: [AuthGuard] },
  { path: 'employe', component: EspaceEmployeComponent, canActivate: [EmployeGuard] },
  { path: 'covoiturage/:id', component: CovoiturageDetailComponent },
  { path: 'saisie/:id', component: SaisieCovoiturageComponent, canActivate: [AuthGuard] },
  { path: 'historique/:id', component: HistoriqueComponent, canActivate: [AuthGuard] },
  { path: 'saisie_avis/:id', component: SaisieAvisComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/accueil', pathMatch: 'full' } // Redirige vers la page par défaut];
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
