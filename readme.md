# NeuroBox

This software is based on the youtube video [Davidran Dallmiller](https://www.youtube.com/watch?v=N3tRFayqVtk).
You may find his own version on his [GitHub](https://github.com/davidrmiller/biosim4).

The goal of the project is to have a neuronal network simulation playgroup. Being able to play with different
parameters and conditions and see how the genetic algorithm reach a possible solution of the problem.

## Main simulation view
![Main simulation view](/images/sim_1.png)

## Network preview
![Network preview](/images/sim_2.png)

# How to run it
You can directly [download the latest zip of the release](https://github.com/bertrandpsi/NeurBox/releases), uncompress the zip and run the EXE directly from your
Windows installation. You don't need any additional software (no need to install .Net or anything else).

# The neuronal network

TO BE DESCRIBED

# Scripting

Some parts like the selection condition can be changed directly within the software by writing piece of C# code.
The code is then compiled at the startup of the simulation using the .Net 6 standard libraries and therefore your
script code will run with the same priviledge and performance and the whole software.

# Technology

The software uses C# / .Net 6 / WPF as development stack.