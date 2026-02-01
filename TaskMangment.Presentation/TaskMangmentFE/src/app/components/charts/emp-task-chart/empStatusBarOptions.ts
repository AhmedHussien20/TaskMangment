import { ChartConfiguration, ChartType } from 'chart.js';

export const empStatusBarOptions: ChartConfiguration['options'] = {
  maintainAspectRatio: false,
  responsive: true,
  plugins: {
    legend: {
      display: true,
      labels: { color: '#77778e' },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      max: 100,             
      ticks: { color: '#77778e', stepSize: 10 },
      grid: { color: 'rgba(119, 119, 142, 0.2)' },
    },
    x: {
      ticks: { color: '#77778e' },
      grid: { display: false },
    },
  },
};

export const taskPercentBarType: ChartType = 'bar';

export const taskPercentBarDataTemplate: ChartConfiguration['data'] = {
  labels: [],
  datasets: [
    {
      label: 'Task %',
      data: [],
      backgroundColor: [], 
      borderColor: [],
      borderWidth: 1,
    },
  ],
};
