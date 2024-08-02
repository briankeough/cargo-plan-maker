import * as React from 'react';
import PropTypes from 'prop-types';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import MenuItem from '@mui/material/MenuItem';
import Drawer from '@mui/material/Drawer';
import Divider from '@mui/material/Divider';
import MenuIcon from '@mui/icons-material/Menu';
import DashboardIcon from '@mui/icons-material/Dashboard';

function AppAppBar(props) {
  const [open, setOpen] = React.useState(false);

  const toggleDrawer = (newOpen) => () => {
    setOpen(newOpen);
  };

  return (
    <div>
      <AppBar
        position="absolute"
        sx={{
          boxShadow: 0,
          bgcolor: 'transparent',
          backgroundImage: 'none',
          mt: 2,
        }}
      >
        <Container maxWidth="lg">
          <Toolbar
            variant="regular"
            sx={(theme) => ({
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              flexShrink: 0,
              //borderRadius: '999px',
              bgcolor:
                theme.palette.mode === 'light'
                  ? 'rgba(255, 255, 255, 0.9)'
                  : 'rgba(0, 0, 0, 0.4)',
              backdropFilter: 'blur(24px)',
              maxHeight: 40,
              border: '1px solid',
              borderColor: 'divider',
              boxShadow:
                theme.palette.mode === 'light'
                  ? `0 0 1px rgba(85, 166, 246, 0.1), 1px 1.5px 2px -1px rgba(85, 166, 246, 0.15), 4px 4px 12px -2.5px rgba(85, 166, 246, 0.15)`
                  : '0 0 1px rgba(2, 31, 59, 0.7), 1px 1.5px 2px -1px rgba(2, 31, 59, 0.65), 4px 4px 12px -2.5px rgba(2, 31, 59, 0.65)',
            })}
          >
            <Box
              sx={{
                flexGrow: 1,
                display: 'flex',
                alignItems: 'center',
                ml: '-18px',
                px: 0,
              }}
            >
              <DashboardIcon  sx={{ fill: '#1976d2', margin: '0px 5px 0px 15px', fontSize: '2.5rem' }}
              />
              <Box sx={{ display: { xs: 'none', md: 'flex' } }}>
                <MenuItem
                  onClick={() => props.setCurrentView('CargoPlanMaker')}
                  sx={{ py: '6px', px: '12px' }}
                >
                  <Typography variant="body1" color="text.primary">
                    Cargo Plan Maker
                  </Typography>
                </MenuItem>
                
                <Divider 
                  sx={{ display: { margin: '-13px 13px !important' } }}
                  orientation="vertical"
                  flexItem 
                />

                <MenuItem
                  onClick={() => props.setCurrentView('ItemSpecifications')}
                  sx={{ py: '6px', px: '12px' }}
                >
                  <Typography variant="body1" color="text.primary">
                    Item Specifications
                  </Typography>
                </MenuItem>

                <MenuItem
                  onClick={() => props.setCurrentView('DestinationSetup')}
                  sx={{ py: '6px', px: '12px' }}
                >
                  <Typography variant="body1" color="text.primary">
                    Destination Limits Setup
                  </Typography>
                </MenuItem>

              </Box>
            </Box>
            <Box sx={{ display: { sm: '', md: 'none' } }}>
              <Button
                variant="text"
                color="primary"
                aria-label="menu"
                onClick={toggleDrawer(true)}
                sx={{ minWidth: '30px', p: '4px' }}
              >
                <MenuIcon />
              </Button>
              <Drawer anchor="right" open={open} onClose={toggleDrawer(false)}>
                <Box
                  sx={{
                    minWidth: '60dvw',
                    p: 2,
                    backgroundColor: 'background.paper',
                    flexGrow: 1,
                  }}
                >
                  <MenuItem onClick={() => props.setCurrentView('CargoPlanMaker')}>
                    Cargo Plan Maker
                  </MenuItem>
                  <MenuItem onClick={() => props.setCurrentView('ItemSpecifications')}>
                    Item Specifications
                  </MenuItem>
                  <MenuItem onClick={() => props.setCurrentView('DestinationSetup')}>
                    Destination Limits Setup
                  </MenuItem>
                </Box>
              </Drawer>
            </Box>
          </Toolbar>
        </Container>
      </AppBar>
    </div>
  );
}

AppAppBar.propTypes = {
  mode: PropTypes.oneOf(['dark', 'light']).isRequired,
  toggleColorMode: PropTypes.func.isRequired,
};

export default AppAppBar;