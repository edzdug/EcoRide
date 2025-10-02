import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ChartConfiguration } from 'chart.js';
import { ViewChild } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';



@Component({
  selector: 'app-espace-admin',
  standalone: false,
  templateUrl: './espace-admin.component.html',
  styleUrl: './espace-admin.component.css'
})
export class EspaceAdminComponent {
  @ViewChild('covoiturageChart') covoiturageChart?: BaseChartDirective;
  @ViewChild('creditChart') creditChart?: BaseChartDirective;

  chartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Nombre de covoiturages',
        fill: true,
        tension: 0.4,
        borderColor: '#007bff',
        backgroundColor: 'rgba(0, 123, 255, 0.3)',
      }
    ]
  };

  chartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      }
    }
  };

  creditChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [] as number[], 
        label: 'Crédits gagnés prévisionnels',
        fill: true,
        tension: 0.4,
        borderColor: '#28a745',
        backgroundColor: 'rgba(40, 167, 69, 0.3)',
      }
    ]

  };


  creditChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      }
    }
  };

  totalCredits: number = 0;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.chargerStats(this.selectedYear, this.selectedMonth);
    this.chargerCredits(this.selectedYear, this.selectedMonth);
  }

  chargerStats(year: number, month: number) {
    this.http.get<{ date: string, count: number }[]>(`/api/Covoiturage/statistiques-par-jour?year=${year}&month=${month}`)
      .subscribe({
        next: (data) => {
          // Générer la liste des jours du mois actuel
          const now = new Date();
          if (year == null) {
            year = now.getFullYear();
          }
          if (month == null) {
            month = now.getMonth() + 1;
          }

          const daysInMonth = new Date(year, month , 0).getDate(); // nombre de jours dans le mois

          const allDays = [];
          for (let day = 1; day <= daysInMonth; day++) {
            allDays.push(`${year}-${(month ).toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`);
          }

          // Transformer les données reçues en map pour un accès rapide
          const dataMap = new Map<string, number>();
          data.forEach(d => dataMap.set(d.date, d.count));

          // Remplir le tableau de valeurs en mettant 0 si pas de données
          this.chartData.labels = allDays;
          this.chartData.datasets[0].data = allDays.map(day => dataMap.get(day) ?? 0);
          
          console.log('Labels:', this.chartData.labels);
          console.log('Data:', this.chartData.datasets[0].data);
        },
        error: (err) => {
          console.error("Erreur de chargement des statistiques", err);
        }
      });
  }

  chargerCredits(year: number, month: number) {
    const now = new Date();
    if (year == null) {
      year = now.getFullYear();
    }
    if (month == null) {
      month = now.getMonth() + 1;
    }
    const daysInMonth = new Date(year!, month , 0).getDate();

    const allDays: string[] = [];
    for (let day = 1; day <= daysInMonth; day++) {
      allDays.push(`${year}-${(month ).toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`);
    }

    // 1. Charger les crédits par jour (pour le graphique)
    this.http.get<{ date: string, totalCredit: number }[]>(`/api/Covoiturage/statistiques-par-jour/credits?year=${year}&month=${month}`)
      .subscribe({
        next: (data) => {
          const dataMap = new Map<string, number>();
          data.forEach(d => dataMap.set(d.date, d.totalCredit));

          this.creditChartData.labels = allDays;
          this.creditChartData.datasets[0].data = allDays.map(day => dataMap.get(day) ?? 0);
          
        },
        error: (err) => {
          console.error("Erreur de chargement des crédits par jour", err);
        }
      });

    // 2. Charger le total des crédits du mois (pour le total affiché)
    this.http.get<number>(`/api/Covoiturage/total-mensuel?year=${year}&month=${month}`)
      .subscribe({
        next: (total) => {
          this.totalCredits = total;
          console.log('Total Crédits du mois (statut=arriver):', this.totalCredits);
        },
        error: (err) => {
          console.error("Erreur de chargement du total des crédits", err);
        }
      });
  }

  selectedYear: number = new Date().getFullYear();
  selectedMonth: number = new Date().getMonth() + 1; // 1 à 12

  rechargerGraphiques() {
    this.chargerStats(this.selectedYear, this.selectedMonth);
    this.chargerCredits(this.selectedYear, this.selectedMonth);
  }


}
