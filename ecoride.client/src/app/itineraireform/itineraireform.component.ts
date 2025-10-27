import { Component, Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, map } from 'rxjs';
import { HttpClient } from '@angular/common/http';

type Form = {
  depart: string;
  destination: string;
  date: Date;
};

@Injectable({ providedIn: 'root' })
export class FormDataService {
  private _form: Form | null = null;

  setForm(form: Form) {
    this._form = form;
  }

  getForm(): Form | null {
    return this._form;
  }
}

@Component({
  selector: 'app-itineraireform',
  standalone: false,
  templateUrl: './itineraireform.component.html',
  styleUrls: ['./itineraireform.component.css']
})
export class ItineraireformComponent {

  form: Form = {
    depart: '',
    destination: '',
    date: new Date()
  };
  // 🔹 Deux champs contrôlés séparément
  departCtrl = new FormControl<string>('');
  destinationCtrl = new FormControl<string>('');

  // 🔹 Deux tableaux de suggestions séparés
  suggestionsDepart: string[] = [];
  suggestionsDestination: string[] = [];

  constructor(
    private router: Router,                      
    private formDataService: FormDataService,
    private http: HttpClient
  ) { }

  ngOnInit() {
    // Suggestion pour le champ départ
    this.departCtrl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(value => this.getSuggestions(value ?? ''))
    ).subscribe(results => this.suggestionsDepart = results);

    // Suggestion pour le champ destination
    this.destinationCtrl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(value => this.getSuggestions(value ?? ''))
    ).subscribe(results => this.suggestionsDestination = results);
  }

  async envoie() {
    this.formDataService.setForm(this.form);     
    this.router.navigate(['/itinerairevue']);         
  }

  getSuggestions(query: string) {
    if (!query || query.length < 3) return []; // n'appelle pas trop tôt
    const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(query)}&limit=5`;
    return this.http.get<any>(url).pipe(
      // on mappe les résultats pour n’extraire que les noms
      map(res => res.features.map((f: any) => f.properties.label))
    );
  }

  choisirDepart(adresse: string) {
    this.form.depart = adresse;
    this.departCtrl.setValue(adresse, { emitEvent: false });
    this.suggestionsDepart = [];
  }

  choisirDestination(adresse: string) {
    this.form.destination = adresse;
    this.destinationCtrl.setValue(adresse, { emitEvent: false });
    this.suggestionsDestination = [];
  }

}
