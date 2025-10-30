import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormDataService } from '../itineraireform/itineraireform.component';
import { Time } from '@angular/common';
import { delay, Observable } from 'rxjs';
import { Router } from '@angular/router';

interface Covoiturage {
  id?: number;
  dateDepart: string;     
  heureDepart: string;
  lieuDepart: string;
  dateArrivee: string;
  heureArrivee: string;
  lieuArrivee: string;
  statut?: string;
  nbPlace: number;
  prixPersonne: number;
  energie?: string;
  noteMinimale?: number;
}


@Component({
  selector: 'app-itinerairevue',
  standalone: false,
  templateUrl: './itinerairevue.component.html',
  styleUrl: './itinerairevue.component.css'
})
export class ItinerairevueComponent implements OnInit {
  //private apiUrl = 'http://localhost:61576/api/covoiturage/GetItiniraireAll';
  form: any;
  public covoiturages: Covoiturage[] = [];
  list: any;
  filtreEnergieElec: boolean = false;
  filtrePrix: boolean = false;
  prixMaximum: number | null = null;
  dureeMaxMinutes: number | null = null;
  noteMinimale?: number;

  constructor(private http: HttpClient, private formDataService: FormDataService, private router: Router) { }


  ngOnInit() {
    this.form = this.formDataService.getForm();
    this.getAll();    
  }

  onEnergieFilterChange(): void {
    this.filtreEnergies = this.filtreEnergieElec ? ['électrique', 'hybride'] : [];
    this.filterCovoiturages();
  }

  getAll() {
    this.http.get<Covoiturage[]>('/api/Covoiturage/GetItiniraireAll')
    .subscribe(
      (data) => {
        // Sort by DateDepart descending
        this.covoiturages = data.sort((a, b) =>
          new Date(b.dateDepart).getTime() - new Date(a.dateDepart).getTime()
        );
        this.covoiturages=this.covoiturages.filter(v=>v.nbPlace != 0)
        console.log("Données reçues : ", this.covoiturages);
        this.filterCovoiturages();
      },
      (error) => {
        console.error('Failed to load covoiturages:', error);
      }
    );
  }

  voirDetail(id: number) {
    this.router.navigate(['/covoiturage', id]);
  }

  filteredCovoiturages: Covoiturage[] = [];

  filtreEnergies: string[] = [];

  filterCovoiturages(): void {
    if (!this.form) return;

    this.filteredCovoiturages = this.covoiturages.filter((c) => {
      const depart = (c.lieuDepart || '').toLowerCase();
      const arrivee = (c.lieuArrivee || '').toLowerCase();
      const formDepart = (this.form.depart || '').toLowerCase();
      const formDestination = (this.form.destination || '').toLowerCase();

      // Correspondance souple : l’adresse contient la ville ou la ville contient une partie de l’adresse
      const sameDepart =
        depart.includes(formDepart) || formDepart.includes(depart);

      const sameDestination =
        arrivee.includes(formDestination) || formDestination.includes(arrivee);

      const cDate = new Date(c.dateDepart);
      const fDate = new Date(this.form.date);

      // ✅ On conserve uniquement la partie date (sans l’heure)
      const sameDay =
        cDate.getFullYear() === fDate.getFullYear() &&
        cDate.getMonth() === fDate.getMonth() &&
        cDate.getDate() === fDate.getDate();

      // ✅ Si la date est après le jour choisi, on le garde comme "futur"
      const afterDate = cDate > fDate;

      // Énergie
      const matchEnergie =
        !this.filtreEnergieElec ||
        ['électrique', 'hybride'].includes((c.energie || '').toLowerCase());

      // Prix
      const matchPrix = this.prixMaximum == null || c.prixPersonne <= this.prixMaximum;

      // Durée
      let dureeOk = true;
      if (this.dureeMaxMinutes != null) {
        // Compose date+heure départ et arrivée en Date
        const departDateTime = new Date(c.dateDepart);
        const [hDep, mDep] = c.heureDepart.split(':').map(Number);
        departDateTime.setHours(hDep, mDep, 0, 0);

        const arriveeDateTime = new Date(c.dateArrivee);
        const [hArr, mArr] = c.heureArrivee.split(':').map(Number);
        arriveeDateTime.setHours(hArr, mArr, 0, 0);

        // Calcul durée en minutes
        const duree = (arriveeDateTime.getTime() - departDateTime.getTime()) / (1000 * 60);

        dureeOk = duree <= this.dureeMaxMinutes;
      }

      // Note
      const matchNote = !this.noteMinimale || (c.noteMinimale ?? 0) >= this.noteMinimale;

      return sameDepart && sameDestination && (sameDay || afterDate) && matchEnergie && matchPrix && dureeOk && matchNote;
    });
  }


}
