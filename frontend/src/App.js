import * as React from 'react';

import CssBaseline from '@mui/material/CssBaseline';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import AppAppBar from './components/AppAppBar';
import CargoPlanMaker from './views/CargoPlanMaker';
import DestinationSetup from './views/DestinationSetup'
import ItemSpecifications from './views/ItemSpecifications'

const defaultTheme = createTheme({});

export default function LandingPage() {

  const [currentView, setCurrentView] = React.useState('CargoPlanMaker');
  
  return (
    <ThemeProvider theme={defaultTheme}>
      <CssBaseline />
      <AppAppBar setCurrentView={setCurrentView} />
      {currentView === 'CargoPlanMaker' && <CargoPlanMaker /> }
      {currentView === 'ItemSpecifications' && <ItemSpecifications /> }
      {currentView === 'DestinationSetup' && <DestinationSetup /> }
    </ThemeProvider>
  );
}